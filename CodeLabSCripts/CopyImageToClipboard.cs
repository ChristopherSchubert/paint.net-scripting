// Name:Copy Image To Clipboard
// Submenu:Chris
// Author:
// Title:Copy Image To Clipboard
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 0; // [0,100] Slider 1 Description
IntSliderControl Amount2 = 0; // [0,100] Slider 2 Description
IntSliderControl Amount3 = 0; // [0,100] Slider 3 Description
#endregion

Surface src;

void Render(Surface dst, Surface src, Rectangle rect)
{
    this.src = src;
    System.Threading.Thread t = new System.Threading.Thread(
        new System.Threading.ThreadStart(CopyImageToClipboard));
    t.SetApartmentState(System.Threading.ApartmentState.STA); 
    t.Start();
    t.Join();
    
}

void CopyImageToClipboard()
{
   var bitmap = src.CreateAliasedBitmap(true);
   
   Clipboard.SetImage(bitmap);
}
