// Name:Region Fill
// Submenu:Chris
// Author:
// Title:Region Fill
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
ListBoxControl Amount1 = 0; // Region 1 Process|None|Border|Fill
ListBoxControl Amount2 = 1; // Region 1 Color Source|Wheel|Primary|Secondary
IntSliderControl Amount3 = 0; // [0,4096] Region 1 Start X (Inclusive)
IntSliderControl Amount4 = 0; // [0,4096] Region 1 Start Y (Inclusive)
IntSliderControl Amount5 = 0; // [0,4096] Region 1 End X (Exclusive)
IntSliderControl Amount6 = 0; // [0,4096] Region 1 End Y (Exclusive)
CheckboxControl Amount7 = false; // [0,1] Region 1 Alpha Only

ListBoxControl Amount8 = 0; // Region 2 Process|None|Border|Fill
ListBoxControl Amount9 = 0; // Region 2 Solor Source|Wheel|Primary|Secondary
IntSliderControl Amount10 = 0; // [0,4096] Region 2 Start X (Inclusive)
IntSliderControl Amount11 = 0; // [0,4096] Region 2 Start Y (Inclusive)
IntSliderControl Amount12 = 0; // [0,4096] Region 2 End X (Exclusive)
IntSliderControl Amount13 = 0; // [0,4096] Region 2 End Y (Exclusive)
CheckboxControl Amount14 = false; // [0,1] Region 2 Alpha Only

ListBoxControl Amount15 = 0; // Region 3 Process|None|Border|Fill
ListBoxControl Amount16 = 1; // Region 3 Color Source|Wheel|Primary|Secondary
IntSliderControl Amount17 = 0; // [0,4096] Region 3 Start X (Inclusive)
IntSliderControl Amount18 = 0; // [0,4096] Region 3 Start Y (Inclusive)
IntSliderControl Amount19 = 0; // [0,4096] Region 3 End X (Exclusive)
IntSliderControl Amount20 = 0; // [0,4096] Region 3 End Y (Exclusive)
CheckboxControl Amount21 = false; // [0,1] Region 3 Alpha Only
ColorWheelControl Amount22 = ColorBgra.FromBgra(0,0,0,255); // [?] Region 1 Color
ColorWheelControl Amount23 = ColorBgra.FromBgra(0,0,0,255); // [?] Region 2 Color
ColorWheelControl Amount24 = ColorBgra.FromBgra(0,0,0,255); // [?] Region 3 Color
#endregion


public class ParameterSet
{
    public int startX;
    public int startY;
    public int endX;
    public int endY;
    public int processType;
    public ColorBgra color;
    public bool alphaOnly;
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    var parameters = new List<ParameterSet>();
    
    if (Amount1 != 0)
    {
        var set = new ParameterSet();
        parameters.Add(set);
        set.processType = Amount1;
        set.startX = Amount3;
        set.startY = Amount4;
        set.endX = Amount5;
        set.endY = Amount6;
        set.color = Amount2 == 0 ? Amount22 : Amount2 == 1 ? EnvironmentParameters.PrimaryColor : EnvironmentParameters.SecondaryColor;
        set.alphaOnly = Amount7;
    }
    
    if (Amount8 != 0)
    {
        var set = new ParameterSet();
        parameters.Add(set);
        set.processType = Amount8;
        set.startX = Amount10;
        set.startY = Amount11;
        set.endX = Amount12;
        set.endY = Amount13;
        set.color = Amount9 == 0 ? Amount23 : Amount9 == 1 ? EnvironmentParameters.PrimaryColor : EnvironmentParameters.SecondaryColor;
        set.alphaOnly = Amount14;
    }
    
    if (Amount15 != 0)
    {
        var set = new ParameterSet();
        parameters.Add(set);
        set.processType = Amount15;
        set.startX = Amount17;
        set.startY = Amount18;
        set.endX = Amount19;
        set.endY = Amount20;
        set.color = Amount16 == 0 ? Amount24 : Amount16 == 1 ? EnvironmentParameters.PrimaryColor : EnvironmentParameters.SecondaryColor;
        set.alphaOnly = Amount21;
    }
    
    if (parameters.Count == 0)
    {
        return;
    }
    
    
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            foreach(var p in parameters)
            {
                if (x >= p.startX && x < p.endX && y>= p.startY && y < p.endY)
                {
                    if (p.processType == 1)
                    {
                        if (!((x == p.startX || x == p.endX-1) && (y == p.startY || y == p.endY-1)))
                        {
                            continue;
                        }
                    }
                    
                    if (p.alphaOnly)
                    {
                        CurrentPixel.A = p.color.A;
                    }
                    else
                    {
                        CurrentPixel = p.color;
                    }
                }
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}
