using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.PlatformSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class PlatformSizeService : IPlatformSizeService
	{
		public SizeRequest GetPlatformSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}
	}
}