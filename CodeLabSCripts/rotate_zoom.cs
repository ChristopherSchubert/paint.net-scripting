// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
#endregion

void PreRender(Surface dst, Surface src)
{
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    var offset = 100;
    
    RotateZoom(dst, src, rect, offset);
}

void RotateZoom(Surface dst, Surface src, Rectangle rect, double offset)
{
    if (IsCancelRequested) return;

   
        RotateZoomEffect effect = new RotateZoomEffect();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(RotateZoomEffect.PropertyNames.Offset, offset);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    
}