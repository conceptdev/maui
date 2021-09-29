using System;
using Android.Views;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, MauiScrollView>
	{
		protected override MauiScrollView CreateNativeView()
		{
			return new MauiScrollView(
				new Android.Views.ContextThemeWrapper(MauiContext!.Context, Resource.Style.scrollViewTheme), null!,
					Resource.Attribute.scrollViewStyle);
		}

		protected override void ConnectHandler(MauiScrollView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.ScrollChange += ScrollChange;
		}

		protected override void DisconnectHandler(MauiScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.ScrollChange -= ScrollChange;
		}

		void ScrollChange(object? sender, AndroidX.Core.Widget.NestedScrollView.ScrollChangeEventArgs e)
		{
			var context = (sender as View)?.Context;

			if (context == null)
			{
				return;
			}

			VirtualView.VerticalOffset = Context.FromPixels(e.ScrollY);
			VirtualView.HorizontalOffset = Context.FromPixels(e.ScrollX);
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.NativeView == null || handler.MauiContext == null)
				return;

			var padding = scrollView.Padding;

			if (padding == Thickness.Zero || scrollView.PresentedContent == null)
			{
				handler.NativeView.UpdateContent(scrollView.PresentedContent, handler.MauiContext);
			}
			else
			{
				var context = handler.MauiContext.Context;

				var currentPaddingShim = handler.NativeView.FindViewWithTag("MAUIPaddingShim") as ContentViewGroup;

				// TODO ezhart Make padding a Func<Thickness>; only add shim if Padding > 0, and if Padding returns to zero just leave the shim
				if (currentPaddingShim != null)
				{
					currentPaddingShim.RemoveAllViews();
					currentPaddingShim.AddView(scrollView.PresentedContent.ToNative(handler.MauiContext));
				}
				else
				{
					var paddingShim = new ContentViewGroup(context!)
					{
						CrossPlatformMeasure = IncludePadding(scrollView.PresentedContent.Measure, padding),
						CrossPlatformArrange = scrollView.PresentedContent.Arrange,
						Tag = "MAUIPaddingShim" // TODO ezhart Make this a constant, replace it above, too
					};

					handler.NativeView.RemoveAllViews();
					paddingShim.AddView(scrollView.PresentedContent.ToNative(handler.MauiContext));
					handler.NativeView.SetContent(paddingShim);
				}
			}
		}

		static Func<double, double, Size> IncludePadding(Func<double, double, Size> internalMeasure, Thickness padding) 
		{
			return (widthConstraint, heightConstraint) => {

				var measurementWidth = widthConstraint - padding.HorizontalThickness;
				var measurementHeight = heightConstraint - padding.VerticalThickness;

				var result = internalMeasure.Invoke(measurementWidth, measurementHeight);
				
				return new Size(result.Width + padding.HorizontalThickness, result.Height + padding.VerticalThickness); 
			};
		}

		public static void MapPadding(ScrollViewHandler handler, IScrollView scrollView)
		{
			//handler.NativeView.SetInternalPadding(scrollView.Padding);
			MapContent(handler, scrollView);
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetVerticalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetOrientation(scrollView.Orientation);
		}

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is not ScrollToRequest request)
			{
				return;
			}

			var context = handler.NativeView.Context;

			if (context == null)
			{
				return;
			}

			var horizontalOffsetDevice = (int)context.ToPixels(request.HoriztonalOffset);
			var verticalOffsetDevice = (int)context.ToPixels(request.VerticalOffset);

			handler.NativeView.ScrollTo(horizontalOffsetDevice, verticalOffsetDevice,
				request.Instant, () => handler.VirtualView.ScrollFinished());
		}
	}
}
