using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.Layouts.LayoutManager;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScrollView, IContentView
	{
		object IContentView.Content => Content;
		IView IContentView.PresentedContent => Content;

		double IScrollView.HorizontalOffset
		{
			get => ScrollX;
			set
			{
				if (ScrollX != value)
				{
					SetScrolledPosition(value, ScrollY);
				}
			}
		}

		double IScrollView.VerticalOffset
		{
			get => ScrollY;
			set
			{
				if (ScrollY != value)
				{
					SetScrolledPosition(ScrollX, value);
				}
			}
		}

		void IScrollView.RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
		{
			var request = new ScrollToRequest(horizontalOffset, verticalOffset, instant);
			Handler?.Invoke(nameof(IScrollView.RequestScrollTo), request);
		}

		void IScrollView.ScrollFinished() => SendScrollFinished();

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			// Measure the ScrollView itself (ComputeDesiredSize will account for the ScrollView margins)
			var defaultSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);

			// Account for the ScrollView's margin/padding and use the rest of the available space to measure the actual Content
			var contentWidthConstraint = widthConstraint - Margin.HorizontalThickness - Padding.HorizontalThickness;
			var contentHeightConstraint = heightConstraint - Margin.VerticalThickness - Padding.VerticalThickness;
			(this as IContentView).CrossPlatformMeasure(contentWidthConstraint, contentHeightConstraint);

			// TODO ezhart Verify this next statement; do we really need to re-add the content margin?:
			// The value from ComputeDesiredSize won't account for any margins on the Content; we'll need to do that manually 
			// And we'll use ResolveConstraints to make sure we're sticking within and explicit Height/Width values or externally
			// imposed constraints

			// Retrieve any explicit size values
			var width = (this as IView).Width;
			var height = (this as IView).Height;

			Thickness contentMargin = (this as IContentView)?.PresentedContent?.Margin ?? Thickness.Zero;
			var desiredWidth = ResolveConstraints(widthConstraint, width, defaultSize.Width + contentMargin.HorizontalThickness);
			var desiredHeight = ResolveConstraints(heightConstraint, height, defaultSize.Height + contentMargin.VerticalThickness);

			DesiredSize = new Size(desiredWidth, desiredHeight);
			return DesiredSize;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (Content == null)
			{
				return Size.Zero;
			}

			if (Content is not IView content)
			{
				return Content.DesiredSize;
			}

			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Neither:
				case ScrollOrientation.Both:
					heightConstraint = double.PositiveInfinity;
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Vertical:
				default:
					heightConstraint = double.PositiveInfinity;
					break;
			}

			content.Measure(widthConstraint, heightConstraint);
			ContentSize = content.DesiredSize;

			return ContentSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);

			(this as IContentView).CrossPlatformArrange(Frame);

			return Frame.Size;
		}

		Size IContentView.CrossPlatformArrange(Rectangle bounds)
		{
			if (Content is IView content)
			{
				var padding = Padding;

				// Normally we'd just want the content to be arranged within the ContentView's Frame,
				// but ScrollView content might be larger than the ScrollView itself (for obvious reasons)
				// So in each dimension, we assume the larger of the two values.
				bounds.Width = Math.Max(Frame.Width, content.DesiredSize.Width + padding.HorizontalThickness);
				bounds.Height = Math.Max(Frame.Height, content.DesiredSize.Height + padding.VerticalThickness);

				(this as IContentView).ArrangeContent(bounds);
			}

			return bounds.Size;
		}
	}
}
