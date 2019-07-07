// Name:BaseColor-Transparency
// Submenu:Chris_Batch
// Author:
// Title:BaseColor-Transparency
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
FilenameControl Amount1 = @"C:\Users\Chris\Documents\Unity3D\Animal_Assets\Work\Textures\Flora\Trees\Leaves\Modified\ash_black_branch_1.png"; // Input Directory|png
FilenameControl Amount2 = @"C:\Users\Chris\Documents\Unity3D\Animal_Assets\Work\Textures\Flora\Trees\Leaves\Maps\ash_black_branch_1_ao.png"; // Output Directory|png
TextboxControl Amount3 = "_bc-t"; // [0,255] Output Postfix
CheckboxControl Amount4 = false; // [0,1] Process
CheckboxControl Amount5 = false; // [0,1] Overwrite
CheckboxControl Amount6 = false; // [0,1] Continue On Error
#endregion

private bool first = true;

unsafe void Run(Surface dst)
{
    if (first)
    { 
        Debug.WriteLine("Will not process first iteration.");
        Amount4 = false;
        first = false;
        return;
    }

    if (!process)
    {
        Debug.WriteLine("Will not process.");
        return;
    }
    if (string.IsNullOrWhiteSpace(inputFileDirectory) || string.IsNullOrWhiteSpace(outputFileDirectory))
    {
        Debug.WriteLine("No path available.");
        return;
    }


    var inputDirectory = string.Empty;
    var outputDirectory = string.Empty;

    if (Directory.Exists(inputFileDirectory))
    {
        inputDirectory = inputFileDirectory;
    }
    else
    {
        inputDirectory = Path.GetDirectoryName(inputFileDirectory);

        if (!Directory.Exists(inputDirectory))
        {
            Debug.WriteLine(string.Format("Path does not exist: {0}.", inputDirectory));
            return;
        }
    }

    if (Directory.Exists(outputFileDirectory))
    {
        outputDirectory = outputFileDirectory;
    }
    else
    {
        outputDirectory = Path.GetDirectoryName(outputFileDirectory);

        if (!Directory.Exists(outputDirectory))
        {
            Debug.WriteLine(string.Format("Path does not exist: {0}.", outputDirectory));
            return;
        }
    }

    Debug.WriteLine("Processing...");

    foreach(var filePath in Directory.GetFiles(inputDirectory))
    {
        if (IsCancelRequested)
        {
            return;
        }

        if (!filePath.EndsWith(".png"))
        {
            continue;
        }

        var fileName = string.Format("{0}{1}.png", Path.GetFileNameWithoutExtension(filePath), outputPostfix);
        var outputFilePath = Path.Combine(outputDirectory, fileName);

        if (File.Exists(outputFilePath))
        {
            if (!overwrite)
            {
                continue;
            }
        }

        using(var iStream = File.Open(filePath, FileMode.Open))
        using (var iImage = Image.FromStream(iStream))
        using (var iSurface = Surface.CopyFromGdipImage(iImage, false))
        using (var working = new Surface(iSurface.Size))
        using (var working2 = new Surface(iSurface.Size))
        using (var working3 = new Surface(iSurface.Size))
        using (var working4 = new Surface(iSurface.Size))
        using (var oSurface = new Surface(iSurface.Size))
        {
            width = iSurface.Width;
            height = iSurface.Height;

            selection = new Rectangle(0, 0, iSurface.Width, iSurface.Height);
            Debug.WriteLine(string.Format("{0}: [Width: {1}] [Height: {2}]", filePath, iSurface.Width, iSurface.Height));

            int sum = 0;

            for (int y = selection.Top; y < selection.Bottom; y++)
            {
                ColorBgra* iPtr = iSurface.GetPointAddressUnchecked(selection.Left, y);

                for (int x = selection.Left; x < selection.Right; x++)
                {
                    ColorBgra work = *iPtr;

                    if (work.A == byte.MaxValue)
                    {
                        sum += 1;
                    }

                    iPtr++;
                }
            }

            AntiAlias(working4, iSurface, selection, 1, 1, 1);

            if (sum > .1f * (width*height)) {  AntiAlias(working4, working4, selection, 3, 2, 1); } 
            if (sum > .2f * (width*height)) {  AntiAlias(working4, working4, selection, 3, 2, 1); } 
            if (sum > .4f * (width*height)) {  AntiAlias(working4, working4, selection, 3, 2, 1); } 
            if (sum > .6f * (width*height)) {  AntiAlias(working4, working4, selection, 3, 2, 1); } 
            if (sum > .8f * (width*height)) {  AntiAlias(working4, working4, selection, 3, 2, 1); } 

            working.CopySurface(working4);

            for (int y = selection.Top; y < selection.Bottom; y++)
            {
                ColorBgra* workPtr = working.GetPointAddressUnchecked(selection.Left, y);

                for (int x = selection.Left; x < selection.Right; x++)
                {
                    ColorBgra work = *workPtr;

                    if (work.A < byte.MaxValue)
                    {
                        work = ColorBgra.Transparent;
                    }

                    *workPtr = work;

                    workPtr++;
                }
            }

            sum = 0;
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;

            for (int y = selection.Top; y < selection.Bottom; y++)
            {
                ColorBgra* workPtr = working.GetPointAddressUnchecked(selection.Left, y);

                for (int x = selection.Left; x < selection.Right; x++)
                {
                    ColorBgra work = *workPtr;

                    if (work.A == byte.MaxValue)
                    {
                        sum += 1;
                        sumB += work.B;
                        sumG += work.G;
                        sumR += work.R;
                    }

                    workPtr++;
                }
            }

            Debug.WriteLine(string.Format("{0}: [Sum: {1}] [SumR: {2}] [SumG: {3}] [SumB: {4}]", filePath, sum, sumR, sumG, sumB));

            var averageColor = ColorBgra.FromBgr((byte)(int)(sumB / sum), (byte)(int)(sumG / sum), (byte)(int)(sumR / sum));

            int iterations = 2*2*2*2*2;
            var directionModifiersX = new int[] {1, 0, -1, 0};
            var directionModifiersY = new int[] {0, 1, 0, -1};

            working2.CopySurface(working);

            for(int i = 0; i < iterations; i++)
            {
                for(var j = 0; j < 4; j++)
                {
                    working.CopySurface(working2);

                    for (int y = selection.Top; y < selection.Bottom; y++)
                    {
                        ColorBgra* workPtr = working.GetPointAddressUnchecked(selection.Left, y);
                        ColorBgra* work2Ptr = working2.GetPointAddressUnchecked(selection.Left, y);
                        ColorBgra* targetPtr = working.GetPointAddressUnchecked(selection.Left + directionModifiersX[j], y + directionModifiersY[j]);

                        for (int x = selection.Left; x < selection.Right; x++)
                        {
                            if (!(
                                x + directionModifiersX[j] < 0         ||
                            x + directionModifiersX[j] > width - 1 ||
                            y + directionModifiersY[j] < 0         ||
                            y + directionModifiersY[j] > height - 1
                            ))
                            {
                                ColorBgra work = *workPtr;
                                ColorBgra target = *targetPtr;

                                if (target.A == byte.MaxValue && work.A < byte.MaxValue)
                                {
                                    *work2Ptr = target;
                                }
                            }

                            workPtr++;
                            work2Ptr++;
                            targetPtr++;
                        }
                    }
                }
            }

            for (int y = selection.Top; y < selection.Bottom; y++)
            {
                ColorBgra* workPtr = working.GetPointAddressUnchecked(selection.Left, y);
                ColorBgra* work4Ptr = working4.GetPointAddressUnchecked(selection.Left, y);

                for (int x = selection.Left; x < selection.Right; x++)
                {
                    ColorBgra work = *workPtr;
                    ColorBgra work4 = *work4Ptr;

                    if (work.A == byte.MinValue)
                    {
                        work = averageColor;
                    }

                    work.A = work4.A;

                    *workPtr = work;

                    workPtr++;
                    work4Ptr++;
                }
            }


            oSurface.CopySurface(working);

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            using(var oStream = File.Open(outputFilePath, FileMode.Create))
            using (Bitmap bitmap = oSurface.CreateAliasedBitmap(true))
            {
                SavePNG(oStream, bitmap);
            }
        }
    }
}

private void SavePNG(
Stream stream,
Bitmap bitmap
)
{
    using (MemoryStream memoryStream = new MemoryStream())
    {
        bitmap.Save((Stream)memoryStream, System.Drawing.Imaging.ImageFormat.Png);
        memoryStream.Position = 0L;
        byte[] array = memoryStream.ToArray();
        stream.Write(array, 0, array.Length);
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// UI VALUE ASSIGNMENT
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
private void AssignUIValues()
{
    inputFileDirectory.Value = (string)Amount1;
    outputFileDirectory.Value = (string)Amount2;
    outputPostfix.Value = (string)Amount3;
    process.Value = (bool)Amount4;
    overwrite.Value = (bool)Amount5;
}

private void PushUIValues()
{
    Amount1 = inputFileDirectory.Value;
    Amount2 = outputFileDirectory.Value;
    Amount3 = outputPostfix.Value;
    Amount4 = process.Value;
    Amount5 = overwrite.Value;
}


TrackingProperty<string> inputFileDirectory = new TrackingProperty<string>();
TrackingProperty<string> outputFileDirectory = new TrackingProperty<string>();
TrackingProperty<string> outputPostfix = new TrackingProperty<string>();
TrackingProperty<bool> process = new TrackingProperty<bool>();
TrackingProperty<bool> overwrite = new TrackingProperty<bool>();


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// COMMON CODE BELOW
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
unsafe void PreRender(Surface dst, Surface src)
{
    AssignUIValues();
    PushUIValues();

    try 
    {
        Run(dst);
    }
    catch(Exception ex)
    {
        Debug.WriteLine(ex);
    }

    dst.CopySurface(src);
}

void Render(Surface dst, Surface src, Rectangle rect)
{
}


IntelligentColorPalette palette;

Stopwatch processStopwatch = new Stopwatch();
Stopwatch methodStopwatch = new Stopwatch();

Surface working = null;
Surface working2 = null;
Surface working3 = null;
Surface working4 = null;
Rectangle selection = default(Rectangle);
int selectionCenterX;
int selectionCenterY;
ColorBgra primaryColor;
ColorBgra secondaryColor;
IReadOnlyList<ColorBgra> defaultColors;
IReadOnlyList<ColorBgra> currentColors;
float width;
float height;

protected override void OnDispose(bool disposing)
{
    if (disposing)
    {
        // Release any surfaces or effects you've created.
        if (working != null) working.Dispose();
            if (working2 != null) working2.Dispose();
            if (working3 != null) working3.Dispose();
            if (working4 != null) working4.Dispose();
            working = null;
        working2 = null;
        working3 = null;
        working4 = null;
    }

    base.OnDispose(disposing);
}

public class TrackingProperty<T> : IEquatable<TrackingProperty<T>>, IEquatable<T>

{
    private bool wasLastChangeEffective = true;

    private T stored;

    public T Value
    {
        get
        {
            return stored;
        }
        set
        {
            if (stored == null && value == null)
            {
                wasLastChangeEffective = false;
            }
            else if (stored != null && stored.Equals(value))
            {
                wasLastChangeEffective = false;
            }
            else
            {
                wasLastChangeEffective = true;
                stored = value;
            }
        }
    }

    public bool HasChanged
    {
        get
        {
            return wasLastChangeEffective;
        }
    }

    public void Set(T value)
    {
        Value = value;
    }


    // User-defined conversion from Digit to double
    public static implicit operator T(TrackingProperty<T> p)
    {
        return p.Value;
    }

    public override string ToString()
    {
        return stored == null ? string.Empty : stored.ToString();
    }

    bool IEquatable<TrackingProperty<T>>.Equals(TrackingProperty<T> p)
    {
        if (Object.ReferenceEquals(p, null))
        {
            return false;
        }

        if (Object.ReferenceEquals(this, p))
        {
            return true;
        }

        if (this.GetType() != p.GetType())
        {
            return false;
        }

        return (stored.Equals(p.Value));
    }

    bool IEquatable<T>.Equals(T p)
    {
        if (Object.ReferenceEquals(p, null))
        {
            return false;
        }

        if (Object.ReferenceEquals(this, p))
        {
            return true;
        }

        if (this.GetType() != p.GetType())
        {
            return false;
        }

        return (stored.Equals(p));
    }

    public override int GetHashCode()
    {
        return stored == null ? base.GetHashCode()  : stored.GetHashCode();
    }

    public static bool operator ==(TrackingProperty<T> lhs, T rhs)
    {
        // Check for null on left side.
        if (Object.ReferenceEquals(lhs, null))
        {
            if (Object.ReferenceEquals(rhs, null))
            {
                // null == null = true.
                return true;
            }

            // Only the left side is null.
            return false;
        }

        if (lhs.Value == null && rhs == null) return true;
            if (lhs.Value == null || rhs == null) return false;
            return lhs.Value.Equals(rhs);
    }

    public static bool operator !=(TrackingProperty<T> lhs, T rhs)
    {
        return !(lhs == rhs);
    }
}

protected Surface img
{
    get { if (_img != null)
    return _img;
    else
    {
        System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(GetImageFromClipboard));
        t.SetApartmentState(System.Threading.ApartmentState.STA);
        t.Start();
        t.Join();
        return _img;
    }
}
}
private Surface _img = null;
private void GetImageFromClipboard()
{
    Image aimg = null;
    IDataObject clippy;
    try
    {
        // Try to paste PNG data.
        if (Clipboard.ContainsData("PNG"))
        {
            Object png_object = Clipboard.GetData("PNG");
            if (png_object is MemoryStream)
            {
                MemoryStream png_stream = png_object as MemoryStream;
                aimg = Image.FromStream(png_stream);
            }
        }
        else if (Clipboard.ContainsImage())
        {
            aimg = Clipboard.GetImage();
        }
    }
    catch (Exception )
    {
    }
    if (aimg != null)
    {
        _img = Surface.CopyFromGdipImage(aimg);
    }
    else
    {
        _img = null;
    }
}

unsafe void CopySurfaceRectangle(Surface dst, Surface src, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;

            *dstPtr = *srcPtr;

            srcPtr++;
            dstPtr++;
        }
    }
}

private unsafe void StandardizeEdges(Surface dst, Surface src, int edgeWidth)
{
    for(var j = 0; j < 1; j++)
    {
        for (int y = selection.Top; y < selection.Bottom-1; y++)
        {
            ColorBgra* srcPtr = src.GetPointAddressUnchecked(selection.Left, y);
            ColorBgra* dstPtr = dst.GetPointAddressUnchecked(selection.Left, y);

            for (int x = selection.Left; x < selection.Right-1; x++)
            {
                if (IsCancelRequested) return;

                if (!MatchesColor(*srcPtr, ColorBgra.TransparentBlack))
                {
                    var thisColor = *srcPtr;

                    if (MatchesColor(src[x-1,y], ColorBgra.TransparentBlack))
                    {
                        var newColor = StandardDeviationEdge(thisColor, x, y, edgeWidth, 1, 0);

                        if (newColor != thisColor)
                        {
                            *dstPtr = newColor;
                            srcPtr++;
                            dstPtr++;
                            continue;
                        }
                    }

                    if (MatchesColor(src[x+1,y], ColorBgra.TransparentBlack))
                    {
                        var newColor = StandardDeviationEdge(thisColor, x, y, edgeWidth, -1, 0);

                        if (newColor != thisColor)
                        {
                            *dstPtr = newColor;
                            srcPtr++;
                            dstPtr++;
                            continue;
                        }
                    }
                    if (MatchesColor(src[x,y-1], ColorBgra.TransparentBlack))
                    {
                        var newColor = StandardDeviationEdge(thisColor, x, y, edgeWidth, 0, 1);

                        if (newColor != thisColor)
                        {
                            *dstPtr = newColor;
                            srcPtr++;
                            dstPtr++;
                            continue;
                        }
                    }
                    if (MatchesColor(src[x,y+1], ColorBgra.TransparentBlack))
                    {
                        var newColor = StandardDeviationEdge(thisColor, x, y, edgeWidth, 0, -1);

                        if (newColor != thisColor)
                        {
                            *dstPtr = newColor;
                            srcPtr++;
                            dstPtr++;
                            continue;
                        }
                    }
                }

                srcPtr++;
                dstPtr++;
            }
        }
    }
}
private ColorBgra StandardDeviationEdge(ColorBgra thisColor, int x, int y, int edgeWidth, int xIteratorMultiplier, int yIteratorMultiplier)
{
    var originalX = x;
    var originalY = y;

    var sumR = 0;
    var sumG = 0;
    var sumB = 0;

    var skip = false;

    for(var i = 0; i < edgeWidth; i++)
    {
        x += (i * xIteratorMultiplier);
        y += (i * yIteratorMultiplier);

        var color = working[x, y];

        if (color.A == byte.MinValue)
        {
            skip = true;
            break;
        }

        sumR += color.R;
        sumG += color.G;
        sumB += color.B;
    }

    if (!skip)
    {
        var avgR = sumR / edgeWidth;
        var avgG = sumG / edgeWidth;
        var avgB = sumB / edgeWidth;

        var diffR = 0;
        var diffG = 0;
        var diffB = 0;

        x = originalX;
        y = originalY;

        for(var i = 0; i < edgeWidth; i++)
        {
            x += (i * xIteratorMultiplier);
            y += (i * yIteratorMultiplier);

            var color = working[x, y];

            diffR += (color.R - avgR)*(color.R - avgR);
            diffG += (color.G - avgG)*(color.G - avgG);
            diffB += (color.B - avgB)*(color.B - avgB);
        }

        var variationR = diffR / edgeWidth;
        var variationG = diffG / edgeWidth;
        var variationB = diffB / edgeWidth;

        var stdDevR = Math.Sqrt(variationR);
        var stdDevG = Math.Sqrt(variationG);
        var stdDevB = Math.Sqrt(variationB);

        var issueR = (Math.Abs(thisColor.R - avgR) > stdDevR) ? 1 : 0;
        var issueG = (Math.Abs(thisColor.G - avgG) > stdDevG) ? 1 : 0;
        var issueB = (Math.Abs(thisColor.B - avgB) > stdDevB) ? 1 : 0;

        var issues = issueR+issueG+issueB;

        if (issues > 2)
        {
            return ColorBgra.TransparentBlack;
        }
    }

    return thisColor;
}

unsafe void BlendSurfaceSeams(Surface dst, int seamWidthMax)
{
    for (int y = 0; y < dst.Height; y++)
    {
        if (dst[0, y] == dst[dst.Width - 1, y])
        {
            dst[0, y] = ColorBgra.Magenta;
            dst[dst.Width - 1, y] = ColorBgra.Magenta;
            continue;
        }

        var leftSideColors = new Dictionary<ColorBgra, int>();
        var rightSideColors = new Dictionary<ColorBgra, int>();

        var leftSideIndex = 0;
        var rightSideIndex = dst.Width - 1;

        var testDepth = -1;

        var hitLeft = -1;
        var hitRight = -1;

        while (testDepth < seamWidthMax)
        {
            testDepth += 1;

            leftSideIndex += testDepth;
            var leftSideColor = dst[leftSideIndex, y];

            if (!leftSideColors.ContainsKey(leftSideColor))
            {
                leftSideColors.Add(leftSideColor, leftSideIndex);
            }

            rightSideIndex -= testDepth;
            var rightSideColor = dst[rightSideIndex, y];

            if (!rightSideColors.ContainsKey(rightSideColor))
            {
                rightSideColors.Add(rightSideColor, rightSideIndex);
            }

            if (!leftSideColors.ContainsKey(rightSideColor) && !rightSideColors.ContainsKey(leftSideColor))
            {
                continue;
            }
            else if (leftSideColors.ContainsKey(rightSideColor))
            {
                hitLeft = leftSideColors[rightSideColor];
                hitRight = rightSideIndex;
            }
            else if (rightSideColors.ContainsKey(leftSideColor))
            {
                hitLeft = leftSideIndex;
                hitRight = rightSideColors[leftSideColor];
            }
        }

        //var depthTest = 1;
        //while(depthTest < seamWidthMax)
        //{
        //   if (dst[depthTest, y] == dst[dst.Width-depthTest-1, y])
        //   {
        //        break;
        //   }

        //    depthTest++;
        //}

        //if (depthTest == seamWidthMax)
        //{
        //    var leftSide = dst[dst.Width-failsafeBlendDistance, y];
        //    var rightSide = dst[failsafeBlendDistance, y];

        //    var diffR = rightSide.R - leftSide.R;
        //    var diffG = rightSide.G - leftSide.G;
        //    var diffB = rightSide.B - leftSide.B;
        //    var diffA = rightSide.A - leftSide.A;

        //    var blends = new ColorBgra[failsafeBlendDistance*2];

        //    for(var i = 0; i < blends.Length; i++)
        //    {
        //        var normalization = i * (1f / (float)blends.Length);

        //        var color = ColorBgra.FromBgra(
        //            (byte)(leftSide.B + (int)(normalization * diffB)),
        //            (byte)(leftSide.G + (int)(normalization * diffG)),
        //            (byte)(leftSide.R + (int)(normalization * diffR)),
        //            (byte)(leftSide.A + (int)(normalization * diffA))
        //        );

        //        var targetX = dst.Width-failsafeBlendDistance+i;

        //        if (targetX >= dst.Width) targetX -= dst.Width;

        //        dst[targetX, y] = color;
        //        dst[targetX, y] = ColorBgra.White;
        //    }
        //}
        //else
        //{
        //    for(var i = 0; i < depthTest; i++)
        //    {
        //        dst[i, y] = dst[depthTest, y];
        //        dst[i, y] = ColorBgra.Green;
        //        dst[dst.Width-i-1, y] = dst[depthTest, y];
        //        dst[dst.Width-i-1, y] = ColorBgra.Green;
        //    }
        //}
    }
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, double opacity, params BlendModes[] args)
{
    BlendSurfaces(destination, leftHand, rightHand, (float)opacity, args);
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, float opacity, params BlendModes[] args)
{
    var byteOpacity = (byte)(int)(((float)byte.MaxValue) * opacity);

    BlendSurfaces(destination, leftHand, rightHand, byteOpacity, args);
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, byte opacity, params BlendModes[] args)
{
    try
    {
        methodStopwatch.Restart();

        for (var y = 0; y < destination.Height; y++)
        {
            for (var x = 0; x < destination.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return;
                }

                var lhs = leftHand[x, y];
                var rhs = rightHand[x, y];
                if (opacity != 0)
                {
                    rhs.A = opacity;
                }

                var result = lhs;

                foreach (var arg in args)
                {
                    result = BlendedPixel(result, rhs, arg);
                }

                destination[x, y] = result;
            }
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("BlendSurfaces: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, params BlendModes[] args)
{
    BlendSurfaces(destination, leftHand, rightHand, 0, args);
}

void MirrorSurface(Surface dst, Surface src, bool mirrorHorizontally, bool mirrorVertically)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (!mirrorHorizontally && !mirrorVertically)
        {
            if (dst == src)
            {
                return;
            }

            dst.CopySurface(src);
        }

        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (IsCancelRequested) return;

                var srcX = x;
                var srcY = y;

                if (mirrorHorizontally)
                {
                    srcX = (src.Width - 1) - x;
                }

                if (mirrorVertically)
                {
                    srcY = (src.Height - 1) - y;
                }

                target[x, y] = src[srcX, srcY];
            }
        }

        if (dst == src)
        {
            dst.CopySurface(target);
        }

    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("MirrorSurfaces: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void JitterSurface(Surface dst, Surface src, bool jitterHorizontally, bool jitterVertically, int maxJitter, int jitterWidth)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (!jitterHorizontally && !jitterVertically)
        {
            if (dst == src)
            {
                return;
            }

            dst.CopySurface(src);
        }

        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        if (jitterHorizontally)
        {
            for (int y = 0; y < src.Height; y += jitterWidth)
            {
                var jitterAmount = rng.Next(-maxJitter, maxJitter);

                for (var x = 0; x < src.Width; x++)
                {
                    var targetX = x + jitterAmount;

                    if (targetX < 0)
                    {
                        targetX += src.Width;
                    }
                    else if (targetX >= src.Width)
                    {
                        targetX -= src.Width;
                    }

                    for (var subY = 0; subY < jitterWidth; subY++)
                    {
                        if (y + subY >= src.Height)
                        {
                            continue;
                        }
                        target[x, y + subY] = src[targetX, y + subY];
                    }
                }
            }
        }

        if (jitterVertically)
        {
            for (int x = 0; x < src.Width; x += jitterWidth)
            {
                var jitterAmount = rng.Next(-maxJitter, maxJitter);

                for (var y = 0; y < src.Height; y++)
                {
                    var targetY = y + jitterAmount;

                    if (targetY < 0)
                    {
                        targetY += src.Height;
                    }
                    else if (targetY >= src.Height)
                    {
                        targetY -= src.Height;
                    }

                    for (var subX = 0; subX < jitterWidth; subX++)
                    {
                        if (x + subX >= src.Width)
                        {
                            continue;
                        }

                        target[x + subX, y] = src[x + subX, targetY];
                    }
                }
            }
        }


        if (dst == src)
        {
            dst.CopySurface(target);
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("JitterSurface: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void FullyQuarterSurfaceAroundSelectionCenter(Surface dst, Surface src)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (IsCancelRequested) return;

                var srcX = x;
                var srcY = y;

                if (x < selectionCenterX && y < selectionCenterY)
                {
                    srcX += selectionCenterX;
                    srcY += selectionCenterY;

                }
                else if (x >= selectionCenterX && y < selectionCenterY)
                {
                    srcX -= selectionCenterX;
                    srcY += selectionCenterY;
                }
                else if (x < selectionCenterX && y >= selectionCenterY)
                {
                    srcX += selectionCenterX;
                    srcY -= selectionCenterY;
                }
                else // (x >= selectionCenterX && y >= selectionCenterY)
                {
                    srcX -= selectionCenterX;
                    srcY -= selectionCenterY;
                }

                target[x, y] = src[srcX, srcY];
            }
        }

        if (dst == src)
        {
            dst.CopySurface(target);
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("FullyQuarterSurfaceAroundSelectionCenter: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private float Normalize(int thisValue, int minimum, int maximum)
{
    return (((float)(thisValue - minimum)) / ((float)(maximum - minimum)));
}

CloudsEffect cloudEffect = null;
PropertyCollection cloudEffectProperties = null;
PropertyBasedEffectConfigToken cloudEffectConfigToken = null;

void RenderClouds(Surface dst, Surface src, Rectangle rect, int scale, double power)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (gaussianBlurEffect == null)
        {
            cloudEffect = new CloudsEffect();
            cloudEffectProperties = cloudEffect.CreatePropertyCollection();
            cloudEffectConfigToken = new PropertyBasedEffectConfigToken(cloudEffectProperties);
            cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Seed, rng.Next(1, 10));
            cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.BlendMode, new UserBlendOps.NormalBlendOp());
        }

        cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Scale, scale);
        cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Power, power);
        cloudEffect.SetRenderInfo(cloudEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        cloudEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("RenderClouds: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }

}

RotateZoomEffect rotateZoomEffect = null;
PropertyCollection rotateZoomEffectProperties = null;
PropertyBasedEffectConfigToken rotateZoomEffectConfigToken = null;
void RotateZoom(Surface dst, Surface src, Rectangle rect, double tileSize)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (rotateZoomEffect == null)
        {
            rotateZoomEffect = new RotateZoomEffect();
            rotateZoomEffectProperties = rotateZoomEffect.CreatePropertyCollection();
            rotateZoomEffectConfigToken = new PropertyBasedEffectConfigToken(rotateZoomEffectProperties);
        }

        rotateZoomEffectConfigToken.SetPropertyValue(RotateZoomEffect.PropertyNames.Tiling, true);
        rotateZoomEffectConfigToken.SetPropertyValue(RotateZoomEffect.PropertyNames.Zoom, tileSize);
        rotateZoomEffect.SetRenderInfo(rotateZoomEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        rotateZoomEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("RotateZoom: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

GaussianBlurEffect gaussianBlurEffect = null;
PropertyCollection gaussianBlurEffectProperties = null;
PropertyBasedEffectConfigToken gaussianBlurEffectConfigToken = null;
void GaussianBlur(Surface dst, Surface src, Rectangle rect, int radius)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (gaussianBlurEffect == null)
        {
            gaussianBlurEffect = new GaussianBlurEffect();
            gaussianBlurEffectProperties = gaussianBlurEffect.CreatePropertyCollection();
            gaussianBlurEffectConfigToken = new PropertyBasedEffectConfigToken(gaussianBlurEffectProperties);
        }

        gaussianBlurEffectConfigToken.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, radius);
        gaussianBlurEffect.SetRenderInfo(gaussianBlurEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        gaussianBlurEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("GaussianBlur: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}


void AntiAlias(Surface dst, Surface src, Rectangle rect, double sharpness, double gamma, double offset)
{
    if (IsCancelRequested) return;

    sharpness =     DoubleUtil.Clamp(sharpness, 1, 3);
    gamma =         DoubleUtil.Clamp(gamma, .5, 2);
    offset =        DoubleUtil.Clamp(offset, 0, 1);

    try
    {
        methodStopwatch.Restart();

        double num1 = 0.0;
        for (int top = rect.Top; top < rect.Bottom; ++top)
        {
            for (int left = rect.Left; left < rect.Right; ++left)
            {
                ColorBgra colorBgra = src[left, top];


                if (left >= selection.Left + 2 && left < selection.Right - 2 && (top >= selection.Top + 2 && top < selection.Bottom - 2))
                    num1 = (0.7 * (double) src[left + 1, top + 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left, top + 1].A + (double) src[left, top].A + (double) src[left, top - 1].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top - 1].A + 0.35 * (double) src[left + 2, top + 1].A + 0.5 * (double) src[left + 2, top].A + 0.35 * (double) src[left + 2, top - 1].A + 0.35 * (double) src[left + 1, top - 2].A + 0.5 * (double) src[left, top - 2].A + 0.35 * (double) src[left - 1, top - 2].A + 0.35 * (double) src[left - 2, top - 1].A + 0.5 * (double) src[left - 2, top].A + 0.35 * (double) src[left - 2, top + 1].A + 0.35 * (double) src[left - 1, top + 2].A + 0.5 * (double) src[left, top + 2].A + 0.35 * (double) src[left + 1, top + 2].A) / 12.6;
                if (left == selection.Left + 1 && top >= selection.Top + 1 && top < selection.Bottom - 1)
                    num1 = (0.7 * (double) src[left + 1, top + 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left, top + 1].A + (double) src[left, top].A + (double) src[left, top - 1].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top - 1].A + 0.35 * (double) src[left + 2, top + 1].A + 0.5 * (double) src[left + 2, top].A + 0.35 * (double) src[left + 2, top - 1].A) / 9.0;
                if (left + 2 == selection.Right && top - 1 >= selection.Top && top + 1 < selection.Bottom)
                    num1 = (0.7 * (double) src[left + 1, top + 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left, top + 1].A + (double) src[left, top].A + (double) src[left, top - 1].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top - 1].A + 0.35 * (double) src[left - 2, top - 1].A + 0.5 * (double) src[left - 2, top].A + 0.35 * (double) src[left - 2, top + 1].A) / 9.0;
                if (left >= selection.Left + 1 && left < selection.Right - 1 && top == selection.Top + 1)
                    num1 = (0.7 * (double) src[left + 1, top + 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left, top + 1].A + (double) src[left, top].A + (double) src[left, top - 1].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top - 1].A + 0.35 * (double) src[left - 1, top + 2].A + 0.5 * (double) src[left, top + 2].A + 0.35 * (double) src[left + 1, top + 2].A) / 9.0;
                if (left >= selection.Left + 1 && left < selection.Right - 1 && top == selection.Bottom - 2)
                    num1 = (0.7 * (double) src[left + 1, top + 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left, top + 1].A + (double) src[left, top].A + (double) src[left, top - 1].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top - 1].A + 0.35 * (double) src[left + 1, top - 2].A + 0.5 * (double) src[left, top - 2].A + 0.35 * (double) src[left - 1, top - 2].A) / 9.0;
                if (left == selection.Left && top >= selection.Top + 1 && top < selection.Bottom - 1)
                    num1 = ((double) ((int) src[left, top - 1].A + (int) src[left, top].A + (int) src[left, top + 1].A) + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left + 1, top].A + 0.7 * (double) src[left + 1, top + 1].A) / 5.4;
                if (left == selection.Right - 1 && top >= selection.Top + 1 && top < selection.Bottom - 1)
                    num1 = (0.7 * (double) src[left - 1, top - 1].A + (double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left, top - 1].A + (double) src[left, top].A + (double) src[left, top + 1].A) / 5.4;
                if (left >= selection.Left + 1 && left < selection.Right - 1 && top == selection.Top)
                    num1 = ((double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left, top].A + (double) src[left, top + 1].A + 0.7 * (double) src[left + 1, top].A + (double) src[left + 1, top + 1].A) / 5.4;
                if (left >= selection.Left + 1 && left < selection.Right - 1 && top == selection.Bottom - 1)
                    num1 = (0.7 * (double) src[left - 1, top - 1].A + 0.7 * (double) src[left - 1, top].A + (double) src[left, top - 1].A + (double) src[left, top].A + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left + 1, top].A) / 5.4;
                if (left == selection.Left && top == selection.Top)
                    num1 = ((double) ((int) src[left, top].A + (int) src[left, top + 1].A + (int) src[left + 1, top].A) + 0.7 * (double) src[left + 1, top + 1].A) / 3.7;
                if (left == selection.Right - 1 && top == selection.Top)
                    num1 = ((double) src[left - 1, top].A + 0.7 * (double) src[left - 1, top + 1].A + (double) src[left, top].A + (double) src[left, top + 1].A) / 3.7;
                if (left == selection.Left && top == selection.Bottom - 1)
                    num1 = ((double) ((int) src[left, top - 1].A + (int) src[left, top].A) + 0.7 * (double) src[left + 1, top - 1].A + (double) src[left + 1, top].A) / 3.7;
                if (left == selection.Right - 1 && top == selection.Bottom - 1)
                    num1 = (0.7 * (double) src[left - 1, top - 1].A + (double) src[left - 1, top].A + (double) src[left, top - 1].A + (double) src[left, top].A) / 3.7;


                double num2 = (double) byte.MaxValue * Math.Pow(num1 / (double) byte.MaxValue, gamma);
                double num3 = offset * ((double) byte.MaxValue - (double) byte.MaxValue / sharpness);
                if (num3 <= 0.0)
                    num3 = 0.0;
                num1 = sharpness * num2 - num3;
                if (num1 > (double) byte.MaxValue)
                    num1 = (double) byte.MaxValue;
                if (num1 < 0.0)
                    num1 = 0.0;
                colorBgra.A = (byte) num1;
                dst[left, top] = colorBgra;
            }
        }

    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("AntiAlias: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

ReliefEffect reliefEffect = null;
PropertyCollection reliefEffectProperties = null;
PropertyBasedEffectConfigToken reliefEffectConfigToken = null;
void Relief(Surface dst, Surface src, Rectangle rect, double angle)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (reliefEffect == null)
        {
            reliefEffect = new ReliefEffect();
            reliefEffectProperties = reliefEffect.CreatePropertyCollection();
            reliefEffectConfigToken = new PropertyBasedEffectConfigToken(reliefEffectProperties);
        }

        reliefEffectConfigToken.SetPropertyValue(ReliefEffect.PropertyNames.Angle, angle);
        reliefEffect.SetRenderInfo(reliefEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        reliefEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("Relief: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

OilPaintingEffect oilPaintingEffect = null;
PropertyCollection oilPaintingEffectProperties = null;
PropertyBasedEffectConfigToken oilPaintingEffectConfigToken = null;
void OilPainting(Surface dst, Surface src, Rectangle rect, int brushSize, int coarseness)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (oilPaintingEffect == null)
        {
            oilPaintingEffect = new OilPaintingEffect();
            oilPaintingEffectProperties = oilPaintingEffect.CreatePropertyCollection();
            oilPaintingEffectConfigToken = new PropertyBasedEffectConfigToken(oilPaintingEffectProperties);
        }

        oilPaintingEffectConfigToken.SetPropertyValue(OilPaintingEffect.PropertyNames.BrushSize, brushSize);
        oilPaintingEffectConfigToken.SetPropertyValue(OilPaintingEffect.PropertyNames.Coarseness, coarseness);
        oilPaintingEffect.SetRenderInfo(oilPaintingEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        oilPaintingEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("OilPainting: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

BrightnessAndContrastAdjustment brightnessAndContrastAdjustment = null;
PropertyCollection brightnessAndContrastAdjustmentProperties = null;
PropertyBasedEffectConfigToken brightnessAndContrastAdjustmentConfigToken = null;
void BrightnessAndContrast(Surface dst, Surface src, Rectangle rect, int brightness, int contrast)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (brightnessAndContrastAdjustment == null)
        {
            brightnessAndContrastAdjustment = new BrightnessAndContrastAdjustment();
            brightnessAndContrastAdjustmentProperties = brightnessAndContrastAdjustment.CreatePropertyCollection();
            brightnessAndContrastAdjustmentConfigToken = new PropertyBasedEffectConfigToken(brightnessAndContrastAdjustmentProperties);
        }

        brightnessAndContrastAdjustmentConfigToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, brightness);
        brightnessAndContrastAdjustmentConfigToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, contrast);
        brightnessAndContrastAdjustment.SetRenderInfo(brightnessAndContrastAdjustmentConfigToken, new RenderArgs(dst), new RenderArgs(src));

        brightnessAndContrastAdjustment.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("BrightnessAndContrast: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

HueAndSaturationAdjustment hueAndSaturationAdjustment = null;
PropertyCollection hueAndSaturationAdjustmentProperties = null;
PropertyBasedEffectConfigToken hueAndSaturationAdjustmentConfigToken = null;
void HueAndSaturation(Surface dst, Surface src, Rectangle rect, int hue, int saturation)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (hueAndSaturationAdjustment == null)
        {
            hueAndSaturationAdjustment = new HueAndSaturationAdjustment();
            hueAndSaturationAdjustmentProperties = hueAndSaturationAdjustment.CreatePropertyCollection();
            hueAndSaturationAdjustmentConfigToken = new PropertyBasedEffectConfigToken(hueAndSaturationAdjustmentProperties);
        }

        hueAndSaturationAdjustmentConfigToken.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Hue, hue);
        hueAndSaturationAdjustmentConfigToken.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Saturation, saturation);
        hueAndSaturationAdjustment.SetRenderInfo(hueAndSaturationAdjustmentConfigToken, new RenderArgs(dst), new RenderArgs(src));

        hueAndSaturationAdjustment.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("HueAndSaturation: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private ColorBgra GetAverageBGRColor(ColorBgra colorOne, ColorBgra colorTwo)
{
    var avgR = (byte)(int)(((float)colorOne.R + colorTwo.R) / 2f);
    var avgG = (byte)(int)(((float)colorOne.G + colorTwo.G) / 2f);
    var avgB = (byte)(int)(((float)colorOne.B + colorTwo.B) / 2f);

    return ColorBgra.FromBgr(avgB, avgG, avgR);
}

private byte GetTone(ColorBgra color)
{
    var colorSum = (float)(color.R + color.G + color.B);

    return (byte)(int)(colorSum / 3f);
}

private static Random rng = new Random();

public void Shuffle<T>(IList<T> list)
{
    if (IsCancelRequested) return;

    int n = list.Count;
    while (n > 1)
    {
        n--;
        int k = rng.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
    }
}

private void ApplyAlphaThresholdToSurface(Surface source, int thresholdBelowBecomes0, int thresholdAboveBecomes255)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return;
                }

                if (source[x, y].A < thresholdBelowBecomes0)
                {
                    source[x, y] = ColorBgra.FromBgra(
                    source[x, y].B,
                    source[x, y].G,
                    source[x, y].R,
                    byte.MinValue);
                }
                else if (source[x, y].A > thresholdAboveBecomes255)
                {
                    source[x, y] = ColorBgra.FromBgra(
                    source[x, y].B,
                    source[x, y].G,
                    source[x, y].R,
                    byte.MaxValue);
                }

            }
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("ApplyAlphaThresholdToSurface: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private Surface GetSurfaceFromToneBand(Surface source, int toneStart, int toneEnd)
{
    var surface = new Surface(source.Size);

    try
    {
        methodStopwatch.Restart();
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return surface;
                }

                var tone = ((source[x, y].R + source[x, y].R + source[x, y].R) / 3);
                if (tone >= toneStart && tone <= toneEnd)
                {
                    surface[x, y] = source[x, y];
                }
                else
                {
                    surface[x, y] = ColorBgra.TransparentBlack;
                }
            }
        }

        return surface;
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("GetSurfaceFromToneBand: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

public class IntelligentColorPalette
{
    private ColorBgra[][] colorPaletteByHueSegment;

    private int lowToneBandLimit = 0;
    private int highToneBandLimit = 255;

    public void InitializePalette(Surface surface, int colorSampleEfficiency,
    int colorFrequencyEqualizer, int lowColorLimit, int highColorLimit, int colorPaletteLength, int colorPaletteHueSegmentation)
    {
        var alwaysZero = 0;

        var colorCounts = new Dictionary<ColorBgra, int>();
        lowToneBandLimit = lowColorLimit;
        highToneBandLimit = highColorLimit;

        ColorBgra CurrentPixel;
        for (int y = 0; y < surface.Height; y += colorSampleEfficiency)
        {
            for (int x = 0; x < surface.Width; x += colorSampleEfficiency)
            {
                var color = surface[x, y];
                color.A = 255;

                var toneBand = ((surface[x, y].R + surface[x, y].G + surface[x, y].B) / 3);

                if (toneBand < lowColorLimit || toneBand > highColorLimit)
                {
                    continue;
                }

                if (!colorCounts.ContainsKey(color))
                {
                    colorCounts.Add(color, 1);
                }
                else
                {
                    colorCounts[color] += 1;
                }
            }
        }

        var toneBandCount = 2 + (int)Math.Pow(2, colorPaletteLength);

        var prePaletteColorsByToneBand = new List<ColorBgra>[toneBandCount];

        for (var i = 0; i < toneBandCount; i++)
        {
            prePaletteColorsByToneBand[i] = new List<ColorBgra>();
        }

        foreach (var colorCount in colorCounts)
        {
            var band = (colorCount.Key.R + colorCount.Key.G + colorCount.Key.B) / 3f;
            var normalized = band / 255f;
            var final = normalized * toneBandCount;
            var bandIndex = (int)(final);

            if (bandIndex == toneBandCount)
            {
                bandIndex -= 1;
            }

            if (colorCount.Value < colorFrequencyEqualizer)
            {
                prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                continue;
            }
            else
            {
                var newColorCount = colorCount.Value / colorFrequencyEqualizer;

                for (var i = 0; i < newColorCount; i++)
                {
                    prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                }
            }
        }

        var toneBandPalettes = new ColorBgra[toneBandCount][];

        for (var toneBandIndex = 0; toneBandIndex < toneBandPalettes.Length; toneBandIndex++)
        {
            toneBandPalettes[toneBandIndex] = GetColorPalette(prePaletteColorsByToneBand[toneBandIndex].ToArray(), colorPaletteLength);
        }

        colorPaletteByHueSegment = new ColorBgra[colorPaletteHueSegmentation][];

        for (var i = 0; i < colorPaletteByHueSegment.Length; i++)
        {
            colorPaletteByHueSegment[i] = new ColorBgra[toneBandCount];
        }

        for (var hueIndex = 0; hueIndex < colorPaletteByHueSegment.Length; hueIndex++)
        {
            for (var toneBandIndex = 0; toneBandIndex < toneBandCount; toneBandIndex++)
            {
                foreach (var color in toneBandPalettes[toneBandIndex])
                {
                    var colorHueSegmentIndex = GetHueSegmentFromHsvColor(HsvColor.FromColor(color.ToColor()), colorPaletteHueSegmentation);

                    if (colorHueSegmentIndex == hueIndex)
                    {
                        colorPaletteByHueSegment[hueIndex][toneBandIndex] = color;
                        break;
                    }
                }
            }
        }
    }

    public void CopyPaletteToSurface(Surface surface, bool useHalf)
    {
        if (colorPaletteByHueSegment == null)
        {
            return;
        }

        var width = useHalf ? surface.Width / 2 : surface.Width;
        var height = surface.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var xPercentage = (float)x / (float)width;
                var yPercentage = (float)y / (float)height;

                var hueIndex = (int)(yPercentage * colorPaletteByHueSegment.Length);
                if (hueIndex == colorPaletteByHueSegment.Length) hueIndex -= 1;

                var colorIndex = (int)(xPercentage * colorPaletteByHueSegment[hueIndex].Length);
                if (colorIndex == colorPaletteByHueSegment[hueIndex].Length) colorIndex -= 1;

                surface[x, y] = colorPaletteByHueSegment[hueIndex][colorIndex];
            }
        }
    }

    public void ReplaceSurfaceColorsWithPalette(Surface destination, Surface source, Rectangle rect)
    {
        if (colorPaletteByHueSegment == null)
        {
            return;
        }

        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (source[x, y].A == 0) continue;

                var nearestPaletteColor = GetNearestPaletteColor(source[x, y]);

                if (nearestPaletteColor == ColorBgra.TransparentBlack)
                {
                    ForceSetColorAsPaletteColor(source[x, y]);
                    nearestPaletteColor = source[x, y];
                }

                destination[x, y] = nearestPaletteColor;
            }
        }
    }

    public int GetToneSegmentIdFromColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);

        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }

        return colorIndex;
    }

    public int GetHueSegmentIdFromColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        return hueSegment;
    }

    public ColorBgra GetNearestPaletteColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }
        else if (hueSegment >= colorPaletteByHueSegment.Length)
        {
            throw new NotSupportedException("Can't get hue index " + hueSegment.ToString() + " as there are only " + colorPaletteByHueSegment.Length.ToString() + " hues.");
        }

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);

        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }
        else if (colorIndex > palette.Length)
        {
            throw new NotSupportedException("Can't get color index " + colorIndex.ToString() + " as there are only " + palette.Length.ToString() + " palettes.");
        }

        return palette[colorIndex];
    }

    private void ForceSetColorAsPaletteColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }
        else if (hueSegment >= colorPaletteByHueSegment.Length)
        {
            throw new NotSupportedException("Can't get hue index " + hueSegment.ToString() + " as there are only " + colorPaletteByHueSegment.Length.ToString() + " hues.");
        }

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);

        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }
        else if (colorIndex > palette.Length)
        {
            throw new NotSupportedException("Can't get color index " + colorIndex.ToString() + " as there are only " + palette.Length.ToString() + " palettes.");
        }

        palette[colorIndex] = color;
    }

    private int GetHueSegmentFromHsvColor(HsvColor color, int numberOfHueSegments)
    {
        if (color.Hue == 0)
        {
            return 1;
        }
        else if (color.Hue == 360)
        {
            return numberOfHueSegments;
        }

        var segmentSize = 360f / (float)numberOfHueSegments;

        for (var i = 0; i < numberOfHueSegments; i++)
        {
            if (color.Hue > segmentSize * i && color.Hue <= segmentSize * (i + 1))
            {
                return i;
            }
        }

        return 0;
    }

    private ColorBgra Spread(ColorBgra[] colors)
    {
        var minimum = new ColorBgra();
        minimum.R = 255;
        minimum.G = 255;
        minimum.B = 255;
        var maximum = new ColorBgra();
        var result = new ColorBgra();

        foreach (var color in colors)
        {
            if (color.R < minimum.R) { minimum.R = color.R; }
            if (color.R > maximum.R) { maximum.R = color.R; }
            if (color.G < minimum.G) { minimum.G = color.G; }
            if (color.G > maximum.G) { maximum.G = color.G; }
            if (color.B < minimum.B) { minimum.B = color.B; }
            if (color.B > maximum.B) { maximum.B = color.B; }
        }

        result.R = (byte)(maximum.R - minimum.R);
        result.G = (byte)(maximum.G - minimum.G);
        result.B = (byte)(maximum.B - minimum.B);

        return result;
    }

    private Tuple<ColorBgra[], ColorBgra[]> Partition(ColorBgra[] colors)
    {
        var spread = Spread(colors);

        var colorList = colors.ToList();

        colorList.Sort((x, y) =>
        {
            if (spread.R >= spread.G && spread.R >= spread.B)
            {
                return y.R.CompareTo(x.R);
            }
            else if (spread.G >= spread.R && spread.G >= spread.B)
            {
                return y.G.CompareTo(x.G);
            }
            else
            {
                return y.B.CompareTo(x.B);
            }
            });

            var result = new Tuple<ColorBgra[], ColorBgra[]>
            (
            colorList.Take(colorList.Count / 2).ToArray(),
            colorList.Skip(colorList.Count / 2).ToArray()
            );

            return result;
        }

        private ColorBgra Average(ColorBgra[] colors)
        {
            if (colors.Length == 0)
            {
                return new ColorBgra();
            }

            var sumR = 0;
            var sumG = 0;
            var sumB = 0;

            foreach (var color in colors)
            {
                sumR += color.R;
                sumG += color.G;
                sumB += color.B;
            }

            return new ColorBgra()
            {
                R = (byte)(sumR / colors.Length),
                G = (byte)(sumG / colors.Length),
                B = (byte)(sumB / colors.Length),
                A = 255
            };
        }

        private ColorBgra[] GetColorPalette(ColorBgra[] colors, int iterations)
        {
            var firstPartitions = Partition(colors);
            var partitions = new List<ColorBgra[]>();

            partitions.Add(firstPartitions.Item1);
            partitions.Add(firstPartitions.Item2);

            for (var i = 0; i < iterations; i++)
            {
                var results = new List<ColorBgra[]>();
                foreach (var partition in partitions)
                {
                    var result = Partition(partition);
                    results.Add(result.Item1);
                    results.Add(result.Item2);
                }

                partitions = results;
            }

            var palette = new ColorBgra[partitions.Count];

            for (var i = 0; i < palette.Length; i++)
            {
                palette[i] = Average(partitions[i]);
            }

            return palette;
        }
    }

    public class FloodFillColorSet
    {
        public Guid floodId;
        public ColorBgra initialColor;
        public Pair<int, int> initialCoordinates;
        public ColorBgra currentColor;
        public Pair<int, int> currentCoordinates;
        public int floodedPixels = 0;
    }

    void FloodFill(int x, int y, Surface dst, Rectangle rect, Func<FloodFillColorSet, bool> comparison, Func<FloodFillColorSet, ColorBgra> fillOperation)
    {
        var stack = new Stack<Tuple<int, int>>();

        stack.Push(new Tuple<int, int>(x, y));

        Tuple<int, int> next;

        var set = new FloodFillColorSet();
        set.floodId = Guid.NewGuid();
        set.initialCoordinates = new Pair<int,int>(x,y);
        set.initialColor = dst[x, y];

        //while(false)
        while (stack.Count > 0)
        {
            if (IsCancelRequested) return;

            next = stack.Pop();

            x = next.Item1;
            y = next.Item2;

            if ((x < 0) || (x >= rect.Right)) continue;
                if ((y < 0) || (y >= rect.Bottom)) continue;

            set.currentColor = dst[x, y];
            set.currentCoordinates = Pair.Create(x, y);

            if (comparison(set))
            {
                if (IsCancelRequested) return;

                set.floodedPixels += 1;
                dst[x, y] = fillOperation(set);

                stack.Push(new Tuple<int, int>(x + 1, y));
                stack.Push(new Tuple<int, int>(x, y + 1));
                stack.Push(new Tuple<int, int>(x - 1, y));
                stack.Push(new Tuple<int, int>(x, y - 1));

            }
        }
    }


    void SmoothEdges(Surface dst, Surface src, int radius)
    {
        try
        {
            methodStopwatch.Restart();
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    if (IsCancelRequested) return;

                    int sum = 0;
                    var count = 0f;

                    var rangeStartX = x - radius;
                    var rangeEndX = x + radius;
                    var rangeStartY = y - radius;
                    var rangeEndY = y + radius;

                    if (x < radius)
                    {
                        rangeStartX = 0;
                    }
                    if (x > src.Width - 1 - radius)
                    {
                        rangeEndX = src.Width - 1;
                    }
                    if (y < radius)
                    {
                        rangeStartY = 0;
                    }
                    if (y > src.Height - 1 - radius)
                    {
                        rangeEndY = src.Height - 1;
                    }

                    for (var yIndex = rangeStartY; yIndex <= rangeEndY; yIndex++)
                    {
                        for (var xIndex = rangeStartX; xIndex <= rangeEndX; xIndex++)
                        {
                            sum += src[xIndex, yIndex].A;
                            count += 1;
                        }
                    }

                    var newA = (byte)(int)(((float)sum) / ((float)count));

                    if (newA > 10 && newA < 240)
                    {
                        count += 1;
                        sum += rng.Next(0, 255);
                        newA = (byte)(int)(((float)sum) / ((float)count));
                    }

                    var newColor = src[x, y];
                    newColor.A = newA;
                    dst[x, y] = newColor;
                }
            }
        }
        finally
        {
            methodStopwatch.Stop();
            Debug.WriteLine("SmoothEdges: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
        }
    }

    public enum BlendModes
    {
        Normal,
        Additive,
        Average,
        Blue,
        Color,
        ColorBurn,
        ColorDodge,
        Cyan,
        Darken,
        Difference,
        Divide,
        Exclusion,
        Glow,
        GrainExtract,
        GrainMerge,
        Green,
        HardLight,
        HardMix,
        Hue,
        Lighten,
        LinearBurn,
        LinearDodge,
        LinearLight,
        Luminosity,
        Magenta,
        Multiply,
        Negation,
        Overlay,
        Phoenix,
        PinLight,
        Red,
        Reflect,
        Saturation,
        Screen,
        SignedDifference,
        SoftLight,
        Stamp,
        VividLight,
        Yellow
    }

    private BinaryPixelOp normalOp = null;

    private ColorBgra BlendedPixel(ColorBgra lhs, ColorBgra rhs, BlendModes blendMode)
    {
        if (normalOp == null)
        {
            normalOp = new UserBlendOps.NormalBlendOp();
        }

        switch (blendMode)
        {
            case BlendModes.Normal:
                return normalOp.Apply(lhs, rhs);

                case BlendModes.Additive:
                    byte r1 = Int32Util.ClampToByte(lhs.R + rhs.R);
                    byte g1 = Int32Util.ClampToByte(lhs.G + rhs.G);
                    byte b1 = Int32Util.ClampToByte(lhs.B + rhs.B);
                    byte a1 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b1, g1, r1, a1);

                    case BlendModes.Average:
                        byte r2 = Int32Util.ClampToByte((lhs.R + rhs.R) / 2);
                        byte g2 = Int32Util.ClampToByte((lhs.G + rhs.G) / 2);
                        byte b2 = Int32Util.ClampToByte((lhs.B + rhs.B) / 2);
                        byte a2 = normalOp.Apply(lhs, rhs).A;
                        return ColorBgra.FromBgra(b2, g2, r2, a2);

                        case BlendModes.Blue:
                            byte a3 = normalOp.Apply(lhs, rhs).A;
                            return ColorBgra.FromBgra(rhs.B, lhs.G, lhs.R, a3);

                            //case BlendModes.Clone:
                            //    byte r3 = 0;
                            //    byte g3 = 0;
                            //    byte b3 = 0;
                            //    byte a4 = normalOp.Apply(lhs, rhs).A;
                            //    return ColorBgra.FromBgra(b3, g3, r3, a4);

                            case BlendModes.Color:
                                HsvColor hsvColor1 = HsvColor.FromColor(lhs);
                                HsvColor hsvColor2 = HsvColor.FromColor(rhs);
                                Color color1 = new HsvColor()
                                {
                                    Hue = hsvColor2.Hue,
                                    Saturation = hsvColor2.Saturation,
                                    Value = hsvColor1.Value
                                    }.ToColor();
                                    byte a5 = normalOp.Apply(lhs, rhs).A;
                                    return ColorBgra.FromBgra(color1.B, color1.G, color1.R, a5);

                                    case BlendModes.ColorBurn:
                                        byte r4 = rhs.R == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R << 8) / rhs.R);
                                        byte g4 = rhs.G == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G << 8) / rhs.G);
                                        byte b4 = rhs.B == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B << 8) / rhs.B);
                                        byte a6 = normalOp.Apply(lhs, rhs).A;
                                        return ColorBgra.FromBgra(b4, g4, r4, a6);

                                        case BlendModes.ColorDodge:
                                            byte r5 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.R << 8) / (byte.MaxValue - rhs.R));
                                            byte g5 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.G << 8) / (byte.MaxValue - rhs.G));
                                            byte b5 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.B << 8) / (byte.MaxValue - rhs.B));
                                            byte a7 = normalOp.Apply(lhs, rhs).A;
                                            return ColorBgra.FromBgra(b5, g5, r5, a7);

                                            case BlendModes.Cyan:
                                                byte a8 = normalOp.Apply(lhs, rhs).A;
                                                return ColorBgra.FromBgra(rhs.B, rhs.G, lhs.R, a8);

                                                case BlendModes.Darken:
                                                    byte r6 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R));
                                                    byte g6 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G));
                                                    byte b6 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B));
                                                    byte a9 = normalOp.Apply(lhs, rhs).A;
                                                    return ColorBgra.FromBgra(b6, g6, r6, a9);

                                                    case BlendModes.Difference:
                                                        byte r7 = Int32Util.ClampToByte(Math.Abs(lhs.R - rhs.R));
                                                        byte g7 = Int32Util.ClampToByte(Math.Abs(lhs.G - rhs.G));
                                                        byte b7 = Int32Util.ClampToByte(Math.Abs(lhs.B - rhs.B));
                                                        byte a10 = normalOp.Apply(lhs, rhs).A;
                                                        return ColorBgra.FromBgra(b7, g7, r7, a10);

                                                        case BlendModes.Divide:
                                                            byte r8 = Int32Util.ClampToByte(lhs.R * byte.MaxValue / (rhs.R + 1));
                                                            byte g8 = Int32Util.ClampToByte(lhs.G * byte.MaxValue / (rhs.G + 1));
                                                            byte b8 = Int32Util.ClampToByte(lhs.B * byte.MaxValue / (rhs.B + 1));
                                                            byte a11 = normalOp.Apply(lhs, rhs).A;
                                                            return ColorBgra.FromBgra(b8, g8, r8, a11);

                                                            case BlendModes.Exclusion:
                                                                byte r9 = Int32Util.ClampToByte(lhs.R + rhs.R - 2 * lhs.R * rhs.R / byte.MaxValue);
                                                                byte g9 = Int32Util.ClampToByte(lhs.G + rhs.G - 2 * lhs.G * rhs.G / byte.MaxValue);
                                                                byte b9 = Int32Util.ClampToByte(lhs.B + rhs.B - 2 * lhs.B * rhs.B / byte.MaxValue);
                                                                byte a12 = normalOp.Apply(lhs, rhs).A;
                                                                return ColorBgra.FromBgra(b9, g9, r9, a12);

                                                                //case BlendModes.Freeze:
                                                                //    byte r10 = rhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.R), 2.0)) / (double)rhs.R) : (byte)0;
                                                                //    byte g10 = rhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.G), 2.0)) / (double)rhs.G) : (byte)0;
                                                                //    byte b10 = rhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.B), 2.0)) / (double)rhs.B) : (byte)0;
                                                                //    byte a13 = normalOp.Apply(lhs, rhs).A;
                                                                //    return ColorBgra.FromBgra(b10, g10, r10, a13);

                                                                case BlendModes.Glow:
                                                                    byte r11 = Int32Util.ClampToByte(lhs.R == byte.MaxValue ? byte.MaxValue : rhs.R * rhs.R / (byte.MaxValue - lhs.R));
                                                                    byte g11 = Int32Util.ClampToByte(lhs.G == byte.MaxValue ? byte.MaxValue : rhs.G * rhs.G / (byte.MaxValue - lhs.G));
                                                                    byte b11 = Int32Util.ClampToByte(lhs.B == byte.MaxValue ? byte.MaxValue : rhs.B * rhs.B / (byte.MaxValue - lhs.B));
                                                                    byte a14 = normalOp.Apply(lhs, rhs).A;
                                                                    return ColorBgra.FromBgra(b11, g11, r11, a14);

                                                                    case BlendModes.Green:
                                                                        byte a15 = normalOp.Apply(lhs, rhs).A;
                                                                        return ColorBgra.FromBgra(lhs.B, rhs.G, lhs.R, a15);

                                                                        case BlendModes.GrainExtract:
                                                                            byte r12 = Int32Util.ClampToByte(lhs.R - rhs.R + 128);
                                                                            byte g12 = Int32Util.ClampToByte(lhs.G - rhs.G + 128);
                                                                            byte b12 = Int32Util.ClampToByte(lhs.B - rhs.B + 128);
                                                                            byte a16 = normalOp.Apply(lhs, rhs).A;
                                                                            return ColorBgra.FromBgra(b12, g12, r12, a16);

                                                                            case BlendModes.GrainMerge:
                                                                                byte r13 = Int32Util.ClampToByte(lhs.R + rhs.R - 128);
                                                                                byte g13 = Int32Util.ClampToByte(lhs.G + rhs.G - 128);
                                                                                byte b13 = Int32Util.ClampToByte(lhs.B + rhs.B - 128);
                                                                                byte a17 = normalOp.Apply(lhs, rhs).A;
                                                                                return ColorBgra.FromBgra(b13, g13, r13, a17);

                                                                                case BlendModes.HardLight:
                                                                                    byte a18 = normalOp.Apply(lhs, rhs).A;
                                                                                    byte r14 = rhs.R > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R) * (byte.MaxValue - 2 * (rhs.R - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.R * rhs.R / byte.MaxValue);
                                                                                    byte g14 = rhs.G > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.G * rhs.G / byte.MaxValue);
                                                                                    byte b14 = rhs.B > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.B * rhs.B / byte.MaxValue);
                                                                                    return ColorBgra.FromBgra(b14, g14, r14, a18);

                                                                                    case BlendModes.HardMix:
                                                                                        byte a19 = normalOp.Apply(lhs, rhs).A;
                                                                                        byte r15 = rhs.R >= byte.MaxValue - lhs.R ? byte.MaxValue : (byte)0;
                                                                                        byte g15 = rhs.G >= byte.MaxValue - lhs.G ? byte.MaxValue : (byte)0;
                                                                                        byte b15 = rhs.B >= byte.MaxValue - lhs.B ? byte.MaxValue : (byte)0;
                                                                                        return ColorBgra.FromBgra(b15, g15, r15, a19);

                                                                                        //case BlendModes.Heat:
                                                                                        //    byte r16 = lhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.R), 2.0)) / (double)lhs.R) : (byte)0;
                                                                                        //    byte g16 = lhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.G), 2.0)) / (double)lhs.G) : (byte)0;
                                                                                        //    byte b16 = lhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.B), 2.0)) / (double)lhs.B) : (byte)0;
                                                                                        //    byte a20 = normalOp.Apply(lhs, rhs).A;
                                                                                        //    return ColorBgra.FromBgra(b16, g16, r16, a20);

                                                                                        case BlendModes.Hue:
                                                                                            HsvColor hsvColor3 = HsvColor.FromColor(lhs);
                                                                                            HsvColor hsvColor4 = HsvColor.FromColor(rhs);
                                                                                            Color color2 = new HsvColor()
                                                                                            {
                                                                                                Hue = hsvColor4.Hue,
                                                                                                Saturation = hsvColor3.Saturation,
                                                                                                Value = hsvColor3.Value
                                                                                                }.ToColor();
                                                                                                byte a21 = normalOp.Apply(lhs, rhs).A;
                                                                                                return ColorBgra.FromBgra(color2.B, color2.G, color2.R, a21);

                                                                                                //case BlendModes.Interpolation:
                                                                                                //    byte r17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.R) - 64.0 * Math.Cos(Math.PI * (double)rhs.R));
                                                                                                //    byte g17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.G) - 64.0 * Math.Cos(Math.PI * (double)rhs.G));
                                                                                                //    byte b17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.B) - 64.0 * Math.Cos(Math.PI * (double)rhs.B));
                                                                                                //    byte a22 = normalOp.Apply(lhs, rhs).A;
                                                                                                //    return ColorBgra.FromBgra(b17, g17, r17, a22);

                                                                                                case BlendModes.Lighten:
                                                                                                    byte r18 = Int32Util.ClampToByte((int)Math.Max(lhs.R, rhs.R));
                                                                                                    byte g18 = Int32Util.ClampToByte((int)Math.Max(lhs.G, rhs.G));
                                                                                                    byte b18 = Int32Util.ClampToByte((int)Math.Max(lhs.B, rhs.B));
                                                                                                    byte a23 = normalOp.Apply(lhs, rhs).A;
                                                                                                    return ColorBgra.FromBgra(b18, g18, r18, a23);

                                                                                                    case BlendModes.LinearBurn:
                                                                                                        byte r19 = Int32Util.ClampToByte(lhs.R + rhs.R - byte.MaxValue);
                                                                                                        byte g19 = Int32Util.ClampToByte(lhs.G + rhs.G - byte.MaxValue);
                                                                                                        byte b19 = Int32Util.ClampToByte(lhs.B + rhs.B - byte.MaxValue);
                                                                                                        byte a24 = normalOp.Apply(lhs, rhs).A;
                                                                                                        return ColorBgra.FromBgra(b19, g19, r19, a24);

                                                                                                        case BlendModes.LinearDodge:
                                                                                                            byte r20 = Int32Util.ClampToByte(lhs.R + rhs.R);
                                                                                                            byte g20 = Int32Util.ClampToByte(lhs.G + rhs.G);
                                                                                                            byte b20 = Int32Util.ClampToByte(lhs.B + rhs.B);
                                                                                                            byte a25 = normalOp.Apply(lhs, rhs).A;
                                                                                                            return ColorBgra.FromBgra(b20, g20, r20, a25);

                                                                                                            case BlendModes.LinearLight:
                                                                                                                byte r21 = Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue);
                                                                                                                byte g21 = Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue);
                                                                                                                byte b21 = Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue);
                                                                                                                byte a26 = normalOp.Apply(lhs, rhs).A;
                                                                                                                return ColorBgra.FromBgra(b21, g21, r21, a26);

                                                                                                                case BlendModes.Luminosity:
                                                                                                                    HsvColor hsvColor5 = HsvColor.FromColor(lhs);
                                                                                                                    HsvColor hsvColor6 = HsvColor.FromColor(rhs);
                                                                                                                    Color color3 = new HsvColor()
                                                                                                                    {
                                                                                                                        Hue = hsvColor5.Hue,
                                                                                                                        Saturation = hsvColor5.Saturation,
                                                                                                                        Value = hsvColor6.Value
                                                                                                                        }.ToColor();
                                                                                                                        byte a27 = normalOp.Apply(lhs, rhs).A;
                                                                                                                        return ColorBgra.FromBgra(color3.B, color3.G, color3.R, a27);

                                                                                                                        case BlendModes.Magenta:
                                                                                                                            byte a28 = normalOp.Apply(lhs, rhs).A;
                                                                                                                            return ColorBgra.FromBgra(rhs.B, lhs.G, rhs.R, a28);

                                                                                                                            case BlendModes.Multiply:
                                                                                                                                byte r22 = Int32Util.ClampToByte(lhs.R * rhs.R / byte.MaxValue);
                                                                                                                                byte g22 = Int32Util.ClampToByte(lhs.G * rhs.G / byte.MaxValue);
                                                                                                                                byte b22 = Int32Util.ClampToByte(lhs.B * rhs.B / byte.MaxValue);
                                                                                                                                byte a29 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                return ColorBgra.FromBgra(b22, g22, r22, a29);

                                                                                                                                case BlendModes.Negation:
                                                                                                                                    byte r23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.R - rhs.R));
                                                                                                                                    byte g23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.G - rhs.G));
                                                                                                                                    byte b23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.B - rhs.B));
                                                                                                                                    byte a30 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                    return ColorBgra.FromBgra(b23, g23, r23, a30);

                                                                                                                                    case BlendModes.Overlay:
                                                                                                                                        byte r24 = lhs.R < (byte)128 ? Int32Util.ClampToByte(2 * rhs.R * lhs.R / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.R) * (byte.MaxValue - lhs.R) / byte.MaxValue);
                                                                                                                                        byte g24 = lhs.G < (byte)128 ? Int32Util.ClampToByte(2 * rhs.G * lhs.G / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.G) * (byte.MaxValue - lhs.G) / byte.MaxValue);
                                                                                                                                        byte b24 = lhs.B < (byte)128 ? Int32Util.ClampToByte(2 * rhs.B * lhs.B / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.B) * (byte.MaxValue - lhs.B) / byte.MaxValue);
                                                                                                                                        byte a31 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                        return ColorBgra.FromBgra(b24, g24, r24, a31);

                                                                                                                                        case BlendModes.Phoenix:
                                                                                                                                            byte r25 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R) - (int)Math.Max(lhs.R, rhs.R) + byte.MaxValue);
                                                                                                                                            byte g25 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G) - (int)Math.Max(lhs.G, rhs.G) + byte.MaxValue);
                                                                                                                                            byte b25 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B) - (int)Math.Max(lhs.B, rhs.B) + byte.MaxValue);
                                                                                                                                            byte a32 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                            return ColorBgra.FromBgra(b25, g25, r25, a32);

                                                                                                                                            case BlendModes.PinLight:
                                                                                                                                                byte r26 = 0;
                                                                                                                                                byte g26 = 0;
                                                                                                                                                byte b26 = 0;
                                                                                                                                                if (lhs.R < 2 * rhs.R - byte.MaxValue)
                                                                                                                                                    r26 = Int32Util.ClampToByte(2 * rhs.R - byte.MaxValue);
                                                                                                                                                if (lhs.G < 2 * rhs.G - byte.MaxValue)
                                                                                                                                                    g26 = Int32Util.ClampToByte(2 * rhs.G - byte.MaxValue);
                                                                                                                                                if (lhs.B < 2 * rhs.B - byte.MaxValue)
                                                                                                                                                    b26 = Int32Util.ClampToByte(2 * rhs.B - byte.MaxValue);
                                                                                                                                                if (lhs.R > 2 * rhs.R - byte.MaxValue && lhs.R < 2 * rhs.R)
                                                                                                                                                    r26 = lhs.R;
                                                                                                                                                if (lhs.G > 2 * rhs.G - byte.MaxValue && lhs.G < 2 * rhs.G)
                                                                                                                                                    g26 = lhs.G;
                                                                                                                                                if (lhs.B > 2 * rhs.B - byte.MaxValue && lhs.B < 2 * rhs.B)
                                                                                                                                                    b26 = lhs.B;
                                                                                                                                                if (lhs.R > 2 * rhs.R)
                                                                                                                                                    r26 = Int32Util.ClampToByte(2 * rhs.R);
                                                                                                                                                if (lhs.G > 2 * rhs.G)
                                                                                                                                                    g26 = Int32Util.ClampToByte(2 * rhs.G);
                                                                                                                                                if (lhs.B > 2 * rhs.B)
                                                                                                                                                    b26 = Int32Util.ClampToByte(2 * rhs.B);
                                                                                                                                                byte a33 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                return ColorBgra.FromBgra(b26, g26, r26, a33);

                                                                                                                                                case BlendModes.Red:
                                                                                                                                                    byte a34 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                    return ColorBgra.FromBgra(lhs.B, lhs.G, rhs.R, a34);

                                                                                                                                                    case BlendModes.Reflect:
                                                                                                                                                        byte r27 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.R * lhs.R / (byte.MaxValue - rhs.R));
                                                                                                                                                        byte g27 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.G * lhs.G / (byte.MaxValue - rhs.G));
                                                                                                                                                        byte b27 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.B * lhs.B / (byte.MaxValue - rhs.B));
                                                                                                                                                        byte a35 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                        return ColorBgra.FromBgra(b27, g27, r27, a35);

                                                                                                                                                        case BlendModes.Saturation:
                                                                                                                                                            HsvColor hsvColor7 = HsvColor.FromColor(lhs);
                                                                                                                                                            HsvColor hsvColor8 = HsvColor.FromColor(rhs);
                                                                                                                                                            Color color4 = new HsvColor()
                                                                                                                                                            {
                                                                                                                                                                Hue = hsvColor7.Hue,
                                                                                                                                                                Saturation = hsvColor8.Saturation,
                                                                                                                                                                Value = hsvColor7.Value
                                                                                                                                                                }.ToColor();
                                                                                                                                                                byte a36 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                return ColorBgra.FromBgra(color4.B, color4.G, color4.R, a36);

                                                                                                                                                                case BlendModes.Screen:
                                                                                                                                                                    byte r28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.R) * (byte.MaxValue - rhs.R) >> 8));
                                                                                                                                                                    byte g28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.G) * (byte.MaxValue - rhs.G) >> 8));
                                                                                                                                                                    byte b28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.B) * (byte.MaxValue - rhs.B) >> 8));
                                                                                                                                                                    byte a37 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                    return ColorBgra.FromBgra(b28, g28, r28, a37);

                                                                                                                                                                    case BlendModes.SignedDifference:
                                                                                                                                                                        byte r29 = Int32Util.ClampToByte((lhs.R - rhs.R) / 2 + 128);
                                                                                                                                                                        byte g29 = Int32Util.ClampToByte((lhs.G - rhs.G) / 2 + 128);
                                                                                                                                                                        byte b29 = Int32Util.ClampToByte((lhs.B - rhs.B) / 2 + 128);
                                                                                                                                                                        byte a38 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                        return ColorBgra.FromBgra(b29, g29, r29, a38);

                                                                                                                                                                        //case BlendModes.SoftBurn:
                                                                                                                                                                        //    byte a39 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                        //    byte r30 = lhs.R + rhs.R >= byte.MaxValue ? (rhs.R != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.R)) / rhs.R) : (byte)0) : (lhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.R / (byte.MaxValue - lhs.R)) : (byte)0);
                                                                                                                                                                        //    byte g30 = lhs.G + rhs.G >= byte.MaxValue ? (rhs.G != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.G)) / rhs.G) : (byte)0) : (lhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.G / (byte.MaxValue - lhs.G)) : (byte)0);
                                                                                                                                                                        //    byte b30 = lhs.B + rhs.B >= byte.MaxValue ? (rhs.B != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.B)) / rhs.B) : (byte)0) : (lhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.B / (byte.MaxValue - lhs.B)) : (byte)0);
                                                                                                                                                                        //    return ColorBgra.FromBgra(b30, g30, r30, a39);

                                                                                                                                                                        //case BlendModes.SoftDodge:
                                                                                                                                                                        //    byte a40 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                        //    byte r31 = lhs.R + rhs.R >= byte.MaxValue ? (lhs.R != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.R)) / lhs.R) : (byte)0) : (rhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.R / (byte.MaxValue - rhs.R)) : byte.MaxValue);
                                                                                                                                                                        //    byte g31 = lhs.G + rhs.G >= byte.MaxValue ? (lhs.G != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.G)) / lhs.G) : (byte)0) : (rhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.G / (byte.MaxValue - rhs.G)) : byte.MaxValue);
                                                                                                                                                                        //    byte b31 = lhs.B + rhs.B >= byte.MaxValue ? (lhs.B != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.B)) / lhs.B) : (byte)0) : (rhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.B / (byte.MaxValue - rhs.B)) : byte.MaxValue);
                                                                                                                                                                        //    return ColorBgra.FromBgra(b31, g31, r31, a40);

                                                                                                                                                                        case BlendModes.SoftLight:
                                                                                                                                                                            byte r32 = DoubleUtil.ClampToByte(rhs.R < 128 ? (float)(2 * ((lhs.R >> 1) + 64)) * ((float)rhs.R / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.R >> 1) + 64))) * (double)(byte.MaxValue - rhs.R) / byte.MaxValue));
                                                                                                                                                                            byte g32 = DoubleUtil.ClampToByte(rhs.G < 128 ? (float)(2 * ((lhs.G >> 1) + 64)) * ((float)rhs.G / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.G >> 1) + 64))) * (double)(byte.MaxValue - rhs.G) / byte.MaxValue));
                                                                                                                                                                            byte b32 = DoubleUtil.ClampToByte(rhs.B < 128 ? (float)(2 * ((lhs.B >> 1) + 64)) * ((float)rhs.B / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.B >> 1) + 64))) * (double)(byte.MaxValue - rhs.B) / byte.MaxValue));
                                                                                                                                                                            byte a41 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                            return ColorBgra.FromBgra(b32, g32, r32, a41);

                                                                                                                                                                            case BlendModes.Stamp:
                                                                                                                                                                                byte r33 = Int32Util.ClampToByte(lhs.R + (2 * rhs.R) - byte.MaxValue);
                                                                                                                                                                                byte g33 = Int32Util.ClampToByte(lhs.G + (2 * rhs.G) - byte.MaxValue);
                                                                                                                                                                                byte b33 = Int32Util.ClampToByte(lhs.B + (2 * rhs.B) - byte.MaxValue);
                                                                                                                                                                                byte a42 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                                return ColorBgra.FromBgra(b33, g33, r33, a42);

                                                                                                                                                                                case BlendModes.VividLight:
                                                                                                                                                                                    byte a43 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                                    byte r34 = rhs.R <= (byte)128 ? Int32Util.ClampToByte(lhs.R + (2 * rhs.R) - byte.MaxValue) : Int32Util.ClampToByte(lhs.R + (2 * (rhs.R - 128)));
                                                                                                                                                                                    byte g34 = rhs.G <= (byte)128 ? Int32Util.ClampToByte(lhs.G + (2 * rhs.G) - byte.MaxValue) : Int32Util.ClampToByte(lhs.G + (2 * (rhs.G - 128)));
                                                                                                                                                                                    byte b34 = rhs.B <= (byte)128 ? Int32Util.ClampToByte(lhs.B + (2 * rhs.B) - byte.MaxValue) : Int32Util.ClampToByte(lhs.B + (2 * (rhs.B - 128)));
                                                                                                                                                                                    return ColorBgra.FromBgra(b34, g34, r34, a43);

                                                                                                                                                                                    case BlendModes.Yellow:
                                                                                                                                                                                        byte a44 = normalOp.Apply(lhs, rhs).A;
                                                                                                                                                                                        return ColorBgra.FromBgra(lhs.B, rhs.G, rhs.R, a44);

                                                                                                                                                                                        default:
                                                                                                                                                                                            return normalOp.Apply(lhs, rhs);
                                                                                                                                                                                        }
                                                                                                                                                                                    }

                                                                                                                                                                                    private unsafe void RotateAndTile(Surface dst, Surface src, Pair<double, double> tileCenter, double xZoom, double yZoom, double tiltAngle, double surfaceAngle, double imageAngle,
                                                                                                                                                                                    int tilingMode, int samples)
                                                                                                                                                                                    {
                                                                                                                                                                                        float midpointX = width / 2f;  //num1
                                                                                                                                                                                        float midpointY = height / 2f;  // num2
                                                                                                                                                                                        float tileCenterX = (float)(tileCenter.First * midpointX);  //num3
                                                                                                                                                                                        float tileCenterY = (float)(tileCenter.Second * midpointY);  //num4

                                                                                                                                                                                        double tiltAngleRadians = Math.PI * tiltAngle / 180.0; // num7
                                                                                                                                                                                        double tiltAngleTangent = Math.Tan(tiltAngleRadians); // num8
                                                                                                                                                                                        float tiltAngleSecant = (float)(1.0 / Math.Cos(tiltAngleRadians)); // num9
                                                                                                                                                                                        double newWidth = xZoom * width; //num10
                                                                                                                                                                                        double newHeight = yZoom * height; //num11
                                                                                                                                                                                        float rotationFactor = 1f / (float)Math.Sqrt((double)newHeight * (double)newHeight + (double)newWidth * (double)newWidth) * (float)tiltAngleTangent; //num12
                                                                                                                                                                                        double tileSurfaceRadians = Math.PI * surfaceAngle / 180.0; //num13

                                                                                                                                                                                        if (tiltAngle == 0.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            tileSurfaceRadians = 0.0;
                                                                                                                                                                                        }

                                                                                                                                                                                        float oppositeCosTileSurface = (float)Math.Cos(-tileSurfaceRadians); //num14
                                                                                                                                                                                        float oppositeSineTileSurface = (float)Math.Sin(-tileSurfaceRadians); //num15
                                                                                                                                                                                        double imageSurfaceRadians = Math.PI * imageAngle / 180.0; //num16
                                                                                                                                                                                        float oppositeCosImageSurface = (float)Math.Cos(-imageSurfaceRadians); //num17
                                                                                                                                                                                        float oppositeSineImageSurface = (float)Math.Sin(-imageSurfaceRadians); //num18

                                                                                                                                                                                        int xZoomFactorInt = (int)xZoom;
                                                                                                                                                                                        int yZoomFactorInt = (int)yZoom;

                                                                                                                                                                                        if ((double)xZoom < 1.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            xZoomFactorInt = (int)(1.0 / (double)xZoom);
                                                                                                                                                                                        }
                                                                                                                                                                                        if ((double)yZoom < 1.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            yZoomFactorInt = (int)(1.0 / (double)yZoom);
                                                                                                                                                                                        }

                                                                                                                                                                                        var xSampleCount = Math.Min(xZoomFactorInt, samples);
                                                                                                                                                                                        var ySampleCount = Math.Min(yZoomFactorInt, samples);

                                                                                                                                                                                        var xStepSize = 1f / (float)xSampleCount;
                                                                                                                                                                                        var yStepSize = 1f / (float)ySampleCount;

                                                                                                                                                                                        var xStepStart = (float)(-0.5 * (1.0 - (double)xStepSize));
                                                                                                                                                                                        var yStepStart = (float)(-0.5 * (1.0 - (double)yStepSize));

                                                                                                                                                                                        var Mxw = (float)-((double)tileCenterX + (double)midpointX);
                                                                                                                                                                                        var Myw = (float)-((double)tileCenterY + (double)midpointY);
                                                                                                                                                                                        var Mxx = oppositeCosImageSurface;
                                                                                                                                                                                        var Mxy = -oppositeSineImageSurface;
                                                                                                                                                                                        float xRotation = (float)((double)oppositeCosImageSurface * (double)Mxw - (double)oppositeSineImageSurface * (double)Myw); //num19
                                                                                                                                                                                        var Myx = oppositeSineImageSurface;
                                                                                                                                                                                        var Myy = oppositeCosImageSurface;
                                                                                                                                                                                        Myw = (float)((double)oppositeSineImageSurface * (double)Mxw + (double)oppositeCosImageSurface * (double)Myw);
                                                                                                                                                                                        Mxw = xRotation;
                                                                                                                                                                                        Mxx *= (float)xZoom;
                                                                                                                                                                                        Mxy *= (float)xZoom;
                                                                                                                                                                                        Mxw *= (float)xZoom;
                                                                                                                                                                                        Myx *= (float)yZoom;
                                                                                                                                                                                        Myy *= (float)yZoom;
                                                                                                                                                                                        Myw *= (float)yZoom;
                                                                                                                                                                                        var Mwx = rotationFactor * Myx;
                                                                                                                                                                                        var Mwy = rotationFactor * Myy;
                                                                                                                                                                                        var Mww = (float)((double)rotationFactor * (double)Myw + 1.0);
                                                                                                                                                                                        Myx = tiltAngleSecant * Myx;
                                                                                                                                                                                        Myy = tiltAngleSecant * Myy;
                                                                                                                                                                                        Myw = tiltAngleSecant * Myw;
                                                                                                                                                                                        float xShift = (float)((double)oppositeCosTileSurface * (double)Mxx - (double)oppositeSineTileSurface * (double)Myx); //num20
                                                                                                                                                                                        float yShift = (float)((double)oppositeCosTileSurface * (double)Mxy - (double)oppositeSineTileSurface * (double)Myy); //num21
                                                                                                                                                                                        float wShift = (float)((double)oppositeCosTileSurface * (double)Mxw - (double)oppositeSineTileSurface * (double)Myw); //num22
                                                                                                                                                                                        Myx = (float)((double)oppositeSineTileSurface * (double)Mxx + (double)oppositeCosTileSurface * (double)Myx);
                                                                                                                                                                                        Myy = (float)((double)oppositeSineTileSurface * (double)Mxy + (double)oppositeCosTileSurface * (double)Myy);
                                                                                                                                                                                        Myw = (float)((double)oppositeSineTileSurface * (double)Mxw + (double)oppositeCosTileSurface * (double)Myw);
                                                                                                                                                                                        Mxx = xShift;
                                                                                                                                                                                        Mxy = yShift;
                                                                                                                                                                                        Mxw = wShift;
                                                                                                                                                                                        Mxx += midpointX * Mwx;
                                                                                                                                                                                        Mxy += midpointX * Mwy;
                                                                                                                                                                                        Mxw += midpointX * Mww;
                                                                                                                                                                                        Myx += midpointY * Mwx;
                                                                                                                                                                                        Myy += midpointY * Mwy;
                                                                                                                                                                                        Myw += midpointY * Mww;

                                                                                                                                                                                        for (int y = 0; y < src.Height; y++)
                                                                                                                                                                                        {
                                                                                                                                                                                            ColorBgra* dstPtr = dst.GetPointAddressUnchecked(0, y);

                                                                                                                                                                                            for (int x = 0; x < src.Width; x++)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (IsCancelRequested) return;

                                                                                                                                                                                            *dstPtr = SSpix(src, x, y, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx, Mwy, Mww, xSampleCount, ySampleCount, xStepSize, yStepSize, xStepStart, yStepStart, tilingMode);

                                                                                                                                                                                            dstPtr++;
                                                                                                                                                                                        }
                                                                                                                                                                                    }
                                                                                                                                                                                }

                                                                                                                                                                                private ColorBgra SSpix(Surface src, int x, int y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww,
                                                                                                                                                                                int xSampleCount, int ySampleCount, float xStepSize, float yStepSize, float xStepStart, float yStepStart, int tilingMode)
                                                                                                                                                                                {
                                                                                                                                                                                    ColorBgra colorBgra = ColorBgra.FromBgra(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
                                                                                                                                                                                    int sampleCount = xSampleCount * ySampleCount;
                                                                                                                                                                                    float sumBlue = 0.0f;
                                                                                                                                                                                    float sumGreen = 0.0f;
                                                                                                                                                                                    float sumRed = 0.0f;
                                                                                                                                                                                    float sumAlpha = 0.0f;

                                                                                                                                                                                    for (int ySampleIndex = 0; ySampleIndex < ySampleCount; ++ySampleIndex)
                                                                                                                                                                                    {
                                                                                                                                                                                        float sampleY = (float)((double)(y - selection.Top) + (double)yStepStart + (double)ySampleIndex * (double)yStepSize);

                                                                                                                                                                                        for (int xSampleIndex = 0; xSampleIndex < xSampleCount; ++xSampleIndex)
                                                                                                                                                                                        {
                                                                                                                                                                                            if (IsCancelRequested) return ColorBgra.Black;

                                                                                                                                                                                            float sampleX = (float)((double)(x - selection.Left) + (double)xStepStart + (double)xSampleIndex * (double)xStepSize);
                                                                                                                                                                                            colorBgra = Move(src, sampleX, sampleY, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx, Mwy, Mww, tilingMode);
                                                                                                                                                                                            sumBlue += (float)((int)colorBgra.B * (int)colorBgra.A);
                                                                                                                                                                                            sumGreen += (float)((int)colorBgra.G * (int)colorBgra.A);
                                                                                                                                                                                            sumRed += (float)((int)colorBgra.R * (int)colorBgra.A);
                                                                                                                                                                                            sumAlpha += (float)colorBgra.A;
                                                                                                                                                                                        }
                                                                                                                                                                                    }
                                                                                                                                                                                    if ((double)sumAlpha == 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        return colorBgra;
                                                                                                                                                                                    }

                                                                                                                                                                                    return ColorBgra.FromBgra(
                                                                                                                                                                                    (byte)(int)((double)sumBlue / (double)sumAlpha),
                                                                                                                                                                                    (byte)(int)((double)sumGreen / (double)sumAlpha),
                                                                                                                                                                                    (byte)(int)((double)sumRed / (double)sumAlpha),
                                                                                                                                                                                    (byte)(int)((double)sumAlpha / (double)sampleCount));
                                                                                                                                                                                }

                                                                                                                                                                                private ColorBgra Move(Surface src, float x, float y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww, int tilingMode)
                                                                                                                                                                                {
                                                                                                                                                                                    if (IsCancelRequested) return ColorBgra.Black;

                                                                                                                                                                                    float shiftXAmount = (float)((double)Mxx * (double)x + (double)Mxy * (double)y) + Mxw; //num1
                                                                                                                                                                                    float shiftYAmount = (float)((double)Myx * (double)x + (double)Myy * (double)y) + Myw; //num2
                                                                                                                                                                                    float rotationAmount = (float)((double)Mwx * (double)x + (double)Mwy * (double)y) + Mww; //num3

                                                                                                                                                                                    ColorBgra colorBgra;
                                                                                                                                                                                    if ((double)rotationAmount <= 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        colorBgra = ColorBgra.Transparent;
                                                                                                                                                                                    }
                                                                                                                                                                                    else
                                                                                                                                                                                    {
                                                                                                                                                                                        float inverseRotationAmount = 1f / rotationAmount; //num4
                                                                                                                                                                                        float xRotation = inverseRotationAmount * shiftXAmount; //num5
                                                                                                                                                                                        float yRotation = inverseRotationAmount * shiftYAmount; //num6

                                                                                                                                                                                        bool flag = false;

                                                                                                                                                                                        float absoluteXShift = (float)(int)Math.Abs(xRotation / width); // num7
                                                                                                                                                                                        float absoluteYShift = (float)(int)Math.Abs(yRotation / height); // num8
                                                                                                                                                                                        float halfRotation = width / 2f; //num9

                                                                                                                                                                                        switch (tilingMode)
                                                                                                                                                                                        {
                                                                                                                                                                                            case 0: // reflect
                                                                                                                                                                                                if ((double)xRotation < 0.0)
                                                                                                                                                                                                    xRotation = -xRotation;
                                                                                                                                                                                                if ((double)xRotation >= (double)width)
                                                                                                                                                                                                    xRotation = (double)absoluteXShift % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                                                                                                                                                                                                if ((double)yRotation < 0.0)
                                                                                                                                                                                                    yRotation = -yRotation;
                                                                                                                                                                                                if ((double)yRotation >= (double)height)
                                                                                                                                                                                                {
                                                                                                                                                                                                    yRotation = (double)absoluteYShift % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                                                                                                                                                                                                    break;
                                                                                                                                                                                            }
                                                                                                                                                                                            break;
                                                                                                                                                                                        case 1: // repeat
                                                                                                                                                                                            if ((double)xRotation < 0.0)
                                                                                                                                                                                                xRotation = width - Math.Abs(xRotation % width);
                                                                                                                                                                                            if ((double)xRotation >= (double)width)
                                                                                                                                                                                                xRotation %= width;
                                                                                                                                                                                            if ((double)yRotation < 0.0)
                                                                                                                                                                                                yRotation = height - Math.Abs(yRotation % height);
                                                                                                                                                                                            if ((double)yRotation >= (double)height)
                                                                                                                                                                                            {
                                                                                                                                                                                                yRotation %= height;
                                                                                                                                                                                                break;
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case 2: // reflect brick
                                                                                                                                                                                        if ((double)yRotation >= 0.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            ++absoluteYShift;
                                                                                                                                                                                        }
                                                                                                                                                                                        if ((double)absoluteYShift % 2.0 == 0.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            xRotation = halfRotation + xRotation;
                                                                                                                                                                                        }
                                                                                                                                                                                        float yBrick = (float)(int)Math.Abs(yRotation / height); // num10
                                                                                                                                                                                        float xBrick = (float)(int)Math.Abs(xRotation / width); // num11
                                                                                                                                                                                        if ((double)xRotation < 0.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            xRotation = -xRotation;
                                                                                                                                                                                        }
                                                                                                                                                                                        if ((double)xRotation >= (double)width)
                                                                                                                                                                                        {
                                                                                                                                                                                            xRotation = (double)xBrick % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                                                                                                                                                                                        }
                                                                                                                                                                                        if ((double)yRotation < 0.0)
                                                                                                                                                                                        {
                                                                                                                                                                                            yRotation = -yRotation;
                                                                                                                                                                                        }
                                                                                                                                                                                        if ((double)yRotation >= (double)height)
                                                                                                                                                                                        {
                                                                                                                                                                                            yRotation = (double)yBrick % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                                                                                                                                                                                            break;
                                                                                                                                                                                    }
                                                                                                                                                                                    break;
                                                                                                                                                                                case 3: // repeat brick
                                                                                                                                                                                    if ((double)yRotation >= 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        ++absoluteYShift;
                                                                                                                                                                                    }
                                                                                                                                                                                    if ((double)absoluteYShift % 2.0 == 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        xRotation = halfRotation + xRotation;
                                                                                                                                                                                    }
                                                                                                                                                                                    if ((double)xRotation < 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        xRotation = width - Math.Abs(xRotation % width);
                                                                                                                                                                                    }
                                                                                                                                                                                    xRotation %= width;
                                                                                                                                                                                    if ((double)yRotation < 0.0)
                                                                                                                                                                                    {
                                                                                                                                                                                        yRotation = height - Math.Abs(yRotation % height);
                                                                                                                                                                                    }
                                                                                                                                                                                    yRotation %= height;
                                                                                                                                                                                    break;
                                                                                                                                                                                case 4: // none
                                                                                                                                                                                    if ((double)xRotation < (double)selection.Left || (double)xRotation > (double)selection.Right || ((double)yRotation < (double)selection.Top || (double)yRotation > (double)selection.Bottom))
                                                                                                                                                                                    {
                                                                                                                                                                                        flag = true;
                                                                                                                                                                                        break;
                                                                                                                                                                                }
                                                                                                                                                                                break;
                                                                                                                                                                        }

                                                                                                                                                                        colorBgra = src.GetBilinearSampleClamped(xRotation + (float)selection.Left, yRotation + (float)selection.Top);

                                                                                                                                                                        if (flag)
                                                                                                                                                                        {
                                                                                                                                                                            colorBgra = ColorBgra.Transparent;
                                                                                                                                                                        }
                                                                                                                                                                    }
                                                                                                                                                                    return colorBgra;
                                                                                                                                                                }

                                                                                                                                                                public class PixelRef
                                                                                                                                                                {
                                                                                                                                                                    public int x;
                                                                                                                                                                    public int y;
                                                                                                                                                                    public ColorBgra color;
                                                                                                                                                                }


                                                                                                                                                                public class LocalizedPixelCube
                                                                                                                                                                {
                                                                                                                                                                    private LocalizedPixelCube()
                                                                                                                                                                    {
                                                                                                                                                                    }

                                                                                                                                                                    public ColorBgra Center { get; private set; }
                                                                                                                                                                    public HsvColor CenterHsv { get; private set; }

                                                                                                                                                                    public int CenterX {get; private set;}
                                                                                                                                                                    public int CenterY {get; private set;}
                                                                                                                                                                    public int PixelCount { get; private set; }

                                                                                                                                                                    public byte AverageB {get { return (byte)(int)(sumB / PixelCount); } }
                                                                                                                                                                    public byte AverageG {get { return (byte)(int)(sumG / PixelCount); } }
                                                                                                                                                                    public byte AverageR {get { return (byte)(int)(sumR / PixelCount); } }
                                                                                                                                                                    public byte AverageA {get { return (byte)(int)(sumA / PixelCount); } }
                                                                                                                                                                    public byte AverageH {get { return (byte)(int)(sumH / PixelCount); } }
                                                                                                                                                                    public byte AverageS {get { return (byte)(int)(sumS / PixelCount); } }
                                                                                                                                                                    public byte AverageV {get { return (byte)(int)(sumV / PixelCount); } }

                                                                                                                                                                    public double StdDevB {get { return Math.Sqrt(sqDiffB); } }
                                                                                                                                                                    public double StdDevG {get { return Math.Sqrt(sqDiffG); } }
                                                                                                                                                                    public double StdDevR {get { return Math.Sqrt(sqDiffR); } }
                                                                                                                                                                    public double StdDevA {get { return Math.Sqrt(sqDiffA); } }
                                                                                                                                                                    public double StdDevH {get { return Math.Sqrt(sqDiffH); } }
                                                                                                                                                                    public double StdDevS {get { return Math.Sqrt(sqDiffS); } }
                                                                                                                                                                    public double StdDevV {get { return Math.Sqrt(sqDiffV); } }

                                                                                                                                                                    public double DevB { get { return Math.Abs(Center.B - AverageB) / StdDevB; } }
                                                                                                                                                                    public double DevG { get { return Math.Abs(Center.G - AverageG) / StdDevG; } }
                                                                                                                                                                    public double DevR { get { return Math.Abs(Center.R - AverageR) / StdDevR; } }
                                                                                                                                                                    public double DevA { get { return Math.Abs(Center.A - AverageA) / StdDevA; } }
                                                                                                                                                                    public double DevH { get { return Math.Abs(CenterHsv.Hue - AverageH) / StdDevH; } }
                                                                                                                                                                    public double DevS { get { return Math.Abs(CenterHsv.Saturation - AverageS) / StdDevS; } }
                                                                                                                                                                    public double DevV { get { return Math.Abs(CenterHsv.Value - AverageV) / StdDevV; } }

                                                                                                                                                                    public NeighborInfo neighborInfo = new NeighborInfo();

                                                                                                                                                                    public struct NeighborInfo
                                                                                                                                                                    {
                                                                                                                                                                        public ColorBgra? LeftOne;
                                                                                                                                                                        public ColorBgra? LeftTwo;
                                                                                                                                                                        public ColorBgra? LeftThree;

                                                                                                                                                                        public ColorBgra? UpOne;
                                                                                                                                                                        public ColorBgra? UpTwo;
                                                                                                                                                                        public ColorBgra? UpThree;

                                                                                                                                                                        public ColorBgra? RightOne;
                                                                                                                                                                        public ColorBgra? RightTwo;
                                                                                                                                                                        public ColorBgra? RightThree;

                                                                                                                                                                        public ColorBgra? DownOne;
                                                                                                                                                                        public ColorBgra? DownTwo;
                                                                                                                                                                        public ColorBgra? DownThree;

                                                                                                                                                                        public ColorBgra? UpperLeft;
                                                                                                                                                                        public ColorBgra? UpperRight;
                                                                                                                                                                        public ColorBgra? LowerLeft;
                                                                                                                                                                        public ColorBgra? LowerRight;


                                                                                                                                                                    }

                                                                                                                                                                    public enum NeighborName { LeftOne, LeftTwo, LeftThree, UpOne, UpTwo, UpThree, RightOne, RightTwo, RightThree, DownOne, DownTwo, DownThree, UpperLeft, UpperRight, LowerLeft, LowerRight }

                                                                                                                                                                    public ColorBgra Average { get { return ColorBgra.FromBgra((byte)(int)AverageB, (byte)(int)AverageG, (byte)(int)AverageR, (byte)(int)AverageA); } }
                                                                                                                                                                    public ColorBgra AverageOpaque { get { return ColorBgra.FromBgra((byte)(int)AverageB, (byte)(int)AverageG, (byte)(int)AverageR, byte.MaxValue); } }

                                                                                                                                                                    public int sumB;
                                                                                                                                                                    public int sumG;
                                                                                                                                                                    public int sumR;
                                                                                                                                                                    public int sumA;
                                                                                                                                                                    public int sumH;
                                                                                                                                                                    public int sumS;
                                                                                                                                                                    public int sumV;

                                                                                                                                                                    public byte maxB = byte.MinValue;
                                                                                                                                                                    public byte maxG = byte.MinValue;
                                                                                                                                                                    public byte maxR = byte.MinValue;
                                                                                                                                                                    public byte maxA = byte.MinValue;
                                                                                                                                                                    public int maxH = int.MinValue;
                                                                                                                                                                    public int maxS = int.MinValue;
                                                                                                                                                                    public int maxV = int.MinValue;

                                                                                                                                                                    public byte minB = byte.MaxValue;
                                                                                                                                                                    public byte minG = byte.MaxValue;
                                                                                                                                                                    public byte minR = byte.MaxValue;
                                                                                                                                                                    public byte minA = byte.MaxValue;
                                                                                                                                                                    public int minH = int.MaxValue;
                                                                                                                                                                    public int minS = int.MaxValue;
                                                                                                                                                                    public int minV = int.MaxValue;

                                                                                                                                                                    public float sqDiffB;
                                                                                                                                                                    public float sqDiffG;
                                                                                                                                                                    public float sqDiffR;
                                                                                                                                                                    public float sqDiffA;
                                                                                                                                                                    public float sqDiffH;
                                                                                                                                                                    public float sqDiffS;
                                                                                                                                                                    public float sqDiffV;

                                                                                                                                                                    //private void RemoveSubcube(LocalizedPixelCube subCube)
                                                                                                                                                                    //{
                                                                                                                                                                    //    sumB -= subCube.sumB;
                                                                                                                                                                    //    sumG -= subCube.sumG;
                                                                                                                                                                    //    sumR -= subCube.sumR;
                                                                                                                                                                    //    sumA -= subCube.sumA;
                                                                                                                                                                    //    sumH -= subCube.sumH;
                                                                                                                                                                    //    sumS -= subCube.sumS;
                                                                                                                                                                    //    sumV -= subCube.sumV;

                                                                                                                                                                    //    sqDiffB -= subCube.sqDiffB;
                                                                                                                                                                    //    sqDiffG -= subCube.sqDiffG;
                                                                                                                                                                    //    sqDiffR -= subCube.sqDiffR;
                                                                                                                                                                    //    sqDiffA -= subCube.sqDiffA;
                                                                                                                                                                    //    sqDiffH -= subCube.sqDiffH;
                                                                                                                                                                    //    sqDiffS -= subCube.sqDiffS;
                                                                                                                                                                    //    sqDiffV -= subCube.sqDiffV;

                                                                                                                                                                    //    PixelCount -= subCube.PixelCount;
                                                                                                                                                                    //}

                                                                                                                                                                    //private void AddSubcube(LocalizedPixelCube subCube)
                                                                                                                                                                    //{
                                                                                                                                                                    //    sumB += subCube.sumB;
                                                                                                                                                                    //    sumG += subCube.sumG;
                                                                                                                                                                    //    sumR += subCube.sumR;
                                                                                                                                                                    //    sumA += subCube.sumA;
                                                                                                                                                                    //    sumH += subCube.sumH;
                                                                                                                                                                    //    sumS += subCube.sumS;
                                                                                                                                                                    //    sumV += subCube.sumV;

                                                                                                                                                                    //    sqDiffB += subCube.sqDiffB;
                                                                                                                                                                    //    sqDiffG += subCube.sqDiffG;
                                                                                                                                                                    //    sqDiffR += subCube.sqDiffR;
                                                                                                                                                                    //    sqDiffA += subCube.sqDiffA;
                                                                                                                                                                    //    sqDiffH += subCube.sqDiffH;
                                                                                                                                                                    //    sqDiffS += subCube.sqDiffS;
                                                                                                                                                                    //    sqDiffV += subCube.sqDiffV;

                                                                                                                                                                    //    PixelCount += subCube.PixelCount;
                                                                                                                                                                    //}

                                                                                                                                                                    public static LocalizedPixelCube BuildFromSurface(Surface src, int x, int y, bool excludeOutliers, int sizeLeft, int sizeRight, int sizeUp, int sizeDown)
                                                                                                                                                                    {
                                                                                                                                                                        var cube = new LocalizedPixelCube();

                                                                                                                                                                        cube.CenterX = x;
                                                                                                                                                                        cube.CenterY = y;
                                                                                                                                                                        cube.Center = src[x,y];
                                                                                                                                                                        cube.CenterHsv = HsvColor.FromColor(cube.Center.ToColor());

                                                                                                                                                                        cube.neighborInfo = GetNeighborInfo(src, x, y);

                                                                                                                                                                        var horizontalSize = 1+sizeLeft+sizeRight;
                                                                                                                                                                        var verticalSize = 1+sizeUp+sizeDown;

                                                                                                                                                                        for(var i = 0; i < horizontalSize; i++)
                                                                                                                                                                        {
                                                                                                                                                                            for(var j = 0; j < verticalSize; j++)
                                                                                                                                                                            {
                                                                                                                                                                                var currentX = x - sizeLeft + i;
                                                                                                                                                                                var currentY = y - sizeUp + j;

                                                                                                                                                                                if (currentX < 0 || currentX > src.Width -1 || currentY < 0 || currentY > src.Height -1 || (currentX == x && currentY == y))
                                                                                                                                                                                {
                                                                                                                                                                                    continue;
                                                                                                                                                                                }

                                                                                                                                                                                var pixel = src[currentX,currentY];
                                                                                                                                                                                var hsv = HsvColor.FromColor(pixel.ToColor());

                                                                                                                                                                                cube.PixelCount += 1;
                                                                                                                                                                                cube.sumB += pixel.B;
                                                                                                                                                                                cube.sumG += pixel.G;
                                                                                                                                                                                cube.sumR += pixel.R;
                                                                                                                                                                                cube.sumA += pixel.A;
                                                                                                                                                                                cube.sumH += hsv.Hue;
                                                                                                                                                                                cube.sumS += hsv.Saturation;
                                                                                                                                                                                cube.sumV += hsv.Value;

                                                                                                                                                                                cube.minB = cube.minB > pixel.B ? pixel.B : cube.minB;
                                                                                                                                                                                cube.minG = cube.minG > pixel.G ? pixel.G : cube.minG;
                                                                                                                                                                                cube.minR = cube.minR > pixel.R ? pixel.R : cube.minR;
                                                                                                                                                                                cube.minA = cube.minA > pixel.A ? pixel.A : cube.minA;
                                                                                                                                                                                cube.minH = cube.minH > hsv.Hue ? hsv.Hue : cube.minH;
                                                                                                                                                                                cube.minS = cube.minS > hsv.Saturation ? hsv.Saturation : cube.minS;
                                                                                                                                                                                cube.minV = cube.minV > hsv.Value ? hsv.Value : cube.minV;

                                                                                                                                                                                cube.maxB = cube.maxB < pixel.B ? pixel.B : cube.maxB;
                                                                                                                                                                                cube.maxG = cube.maxG < pixel.G ? pixel.G : cube.maxG;
                                                                                                                                                                                cube.maxR = cube.maxR < pixel.R ? pixel.R : cube.maxR;
                                                                                                                                                                                cube.maxA = cube.maxA < pixel.A ? pixel.A : cube.maxA;
                                                                                                                                                                                cube.maxH = cube.maxH < hsv.Hue ? hsv.Hue : cube.maxH;
                                                                                                                                                                                cube.maxS = cube.maxS < hsv.Saturation ? hsv.Saturation : cube.maxS;
                                                                                                                                                                                cube.maxV = cube.maxV < hsv.Value ? hsv.Value : cube.maxV;
                                                                                                                                                                            }
                                                                                                                                                                        }

                                                                                                                                                                        if (excludeOutliers)
                                                                                                                                                                        {
                                                                                                                                                                            cube.PixelCount -= 2;
                                                                                                                                                                            cube.sumB -= cube.maxB;
                                                                                                                                                                            cube.sumG -= cube.maxG;
                                                                                                                                                                            cube.sumR -= cube.maxR;
                                                                                                                                                                            cube.sumA -= cube.maxA;
                                                                                                                                                                            cube.sumH -= cube.maxH;
                                                                                                                                                                            cube.sumS -= cube.maxS;
                                                                                                                                                                            cube.sumV -= cube.maxV;

                                                                                                                                                                            cube.sumB -= cube.minB;
                                                                                                                                                                            cube.sumG -= cube.minG;
                                                                                                                                                                            cube.sumR -= cube.minR;
                                                                                                                                                                            cube.sumA -= cube.minA;
                                                                                                                                                                            cube.sumH -= cube.minH;
                                                                                                                                                                            cube.sumS -= cube.minS;
                                                                                                                                                                            cube.sumV -= cube.minV;
                                                                                                                                                                        }

                                                                                                                                                                        for(var i = 0; i < horizontalSize; i++)
                                                                                                                                                                        {
                                                                                                                                                                            for(var j = 0; j < verticalSize; j++)
                                                                                                                                                                            {
                                                                                                                                                                                var currentX = x - sizeLeft + i;
                                                                                                                                                                                var currentY = y - sizeUp + j;

                                                                                                                                                                                if (currentX < 0 || currentX > src.Width -1 || currentY < 0 || currentY > src.Height -1 || (currentX == x && currentY == y))
                                                                                                                                                                                {
                                                                                                                                                                                    continue;
                                                                                                                                                                                }

                                                                                                                                                                                var color = src[currentX,currentY];
                                                                                                                                                                                var hsv = HsvColor.FromColor(color.ToColor());

                                                                                                                                                                                cube.sqDiffB += (color.B - cube.AverageB)*(color.B - cube.AverageB);
                                                                                                                                                                                cube.sqDiffG += (color.G - cube.AverageG)*(color.G - cube.AverageG);
                                                                                                                                                                                cube.sqDiffR += (color.R - cube.AverageR)*(color.R - cube.AverageR);
                                                                                                                                                                                cube.sqDiffA += (color.A - cube.AverageA)*(color.A - cube.AverageA);
                                                                                                                                                                                cube.sqDiffH += (hsv.Hue - cube.AverageH)*(hsv.Hue - cube.AverageH);
                                                                                                                                                                                cube.sqDiffS += (hsv.Saturation - cube.AverageS)*(hsv.Saturation - cube.AverageS);
                                                                                                                                                                                cube.sqDiffV += (hsv.Value - cube.AverageV)*(hsv.Value - cube.AverageV);
                                                                                                                                                                            }
                                                                                                                                                                        }

                                                                                                                                                                        if (excludeOutliers)
                                                                                                                                                                        {
                                                                                                                                                                            cube.sqDiffB -= (cube.maxB - cube.AverageB)*(cube.maxB - cube.AverageB);
                                                                                                                                                                            cube.sqDiffG -= (cube.maxG - cube.AverageG)*(cube.maxG - cube.AverageG);
                                                                                                                                                                            cube.sqDiffR -= (cube.maxR - cube.AverageR)*(cube.maxR - cube.AverageR);
                                                                                                                                                                            cube.sqDiffA -= (cube.maxA - cube.AverageA)*(cube.maxA - cube.AverageA);
                                                                                                                                                                            cube.sqDiffH -= (cube.maxH - cube.AverageH)*(cube.maxH - cube.AverageH);
                                                                                                                                                                            cube.sqDiffS -= (cube.maxS - cube.AverageS)*(cube.maxS - cube.AverageS);
                                                                                                                                                                            cube.sqDiffV -= (cube.maxV - cube.AverageV)*(cube.maxV - cube.AverageV);

                                                                                                                                                                            cube.sqDiffB -= (cube.minB - cube.AverageB)*(cube.minB - cube.AverageB);
                                                                                                                                                                            cube.sqDiffG -= (cube.minG - cube.AverageG)*(cube.minG - cube.AverageG);
                                                                                                                                                                            cube.sqDiffR -= (cube.minR - cube.AverageR)*(cube.minR - cube.AverageR);
                                                                                                                                                                            cube.sqDiffA -= (cube.minA - cube.AverageA)*(cube.minA - cube.AverageA);
                                                                                                                                                                            cube.sqDiffH -= (cube.minH - cube.AverageH)*(cube.minH - cube.AverageH);
                                                                                                                                                                            cube.sqDiffS -= (cube.minS - cube.AverageS)*(cube.minS - cube.AverageS);
                                                                                                                                                                            cube.sqDiffV -= (cube.minV - cube.AverageV)*(cube.minV - cube.AverageV);
                                                                                                                                                                        }

                                                                                                                                                                        return cube;
                                                                                                                                                                    }

                                                                                                                                                                    private static NeighborInfo GetNeighborInfo(Surface src, int x, int y)
                                                                                                                                                                    {
                                                                                                                                                                        var neighborInfo = new NeighborInfo()
                                                                                                                                                                        {
                                                                                                                                                                            LeftOne = x > 0 ? (ColorBgra?)src[x-1,y] : (ColorBgra?)null,
                                                                                                                                                                            LeftTwo = x > 1 ? src[x-2,y] : (ColorBgra?)null,
                                                                                                                                                                            LeftThree = x > 2 ? src[x-3,y] : (ColorBgra?)null,
                                                                                                                                                                            RightOne = x < src.Width - 1 ? src[x+1,y] : (ColorBgra?)null,
                                                                                                                                                                            RightTwo = x < src.Width - 2 ? src[x+2,y] : (ColorBgra?)null,
                                                                                                                                                                            RightThree = x < src.Width - 3 ? src[x+3,y] : (ColorBgra?)null,
                                                                                                                                                                            UpOne = y > 0 ? src[x,y-1] : (ColorBgra?)null,
                                                                                                                                                                            UpTwo = y > 1 ? src[x,y-2] : (ColorBgra?)null,
                                                                                                                                                                            UpThree = y > 2 ? src[x,y-3] : (ColorBgra?)null,
                                                                                                                                                                            DownOne = y < src.Height - 3 ? src[x,y+1] : (ColorBgra?)null,
                                                                                                                                                                            DownTwo = y < src.Height - 3 ? src[x,y+2] : (ColorBgra?)null,
                                                                                                                                                                            DownThree = y < src.Height - 3 ? src[x,y+3] : (ColorBgra?)null,
                                                                                                                                                                            UpperLeft = x > 0 && y > 0 ? src[x-1,y-1] : (ColorBgra?)null,
                                                                                                                                                                            UpperRight = x < src.Width - 1 && y > 0 ? src[x+1,y-1] : (ColorBgra?)null,
                                                                                                                                                                            LowerLeft = x > 0 && y < src.Height - 1 ? src[x-1,y+1] : (ColorBgra?)null,
                                                                                                                                                                            LowerRight = x < src.Width - 1 && y < src.Height - 1 ? src[x+1,y+1] : (ColorBgra?)null
                                                                                                                                                                        };

                                                                                                                                                                        return neighborInfo;
                                                                                                                                                                    }

                                                                                                                                                                    public static IEnumerable<LocalizedPixelCube> BuildCubeListFromSurface(Surface src, Rectangle rect, bool excludeOutliers, int size,
                                                                                                                                                                    params Func<ColorBgra, bool>[] exclusions)
                                                                                                                                                                    {
                                                                                                                                                                        return BuildCubeListFromSurface(src, rect, excludeOutliers, size,size,size,size, exclusions);
                                                                                                                                                                    }

                                                                                                                                                                    public static IEnumerable<LocalizedPixelCube> BuildCubeListFromSurface(Surface src, Rectangle rect, bool excludeOutliers, int sizeLeft, int sizeRight, int sizeUp, int sizeDown,
                                                                                                                                                                    params Func<ColorBgra, bool>[] exclusions)
                                                                                                                                                                    {
                                                                                                                                                                        //var horizontalWidth = sizeLeft + 1 + sizeRight;
                                                                                                                                                                        //var verticalWidth = sizeUp + 1 + sizeDown;

                                                                                                                                                                        //if (horizontalWidth == 1 || verticalWidth == 1)
                                                                                                                                                                        //{
                                                                                                                                                                        for(var x = rect.Left; x < rect.Right; x++)
                                                                                                                                                                        for(var y = rect.Top; y < rect.Bottom; y++)
                                                                                                                                                                        {
                                                                                                                                                                            var skip = false;
                                                                                                                                                                            foreach(var exclusion in exclusions)
                                                                                                                                                                            {
                                                                                                                                                                                var color = src[x,y];

                                                                                                                                                                                if (exclusion(color))
                                                                                                                                                                                {
                                                                                                                                                                                    skip = true;
                                                                                                                                                                                    break;
                                                                                                                                                                            }
                                                                                                                                                                        }

                                                                                                                                                                        if (skip) continue;

                                                                                                                                                                        yield return BuildFromSurface(src, x, y, excludeOutliers, sizeLeft, sizeRight, sizeUp, sizeDown);
                                                                                                                                                                    }

                                                                                                                                                                    //yield break;
                                                                                                                                                                    //}

                                                                                                                                                                    //LocalizedPixelCube cube = null;

                                                                                                                                                                    //if (horizontalWidth >= verticalWidth)
                                                                                                                                                                    //{
                                                                                                                                                                    //    for(var y = rect.Top; y < rect.Bottom; y++)
                                                                                                                                                                    //    {
                                                                                                                                                                    //        if (cube != null)
                                                                                                                                                                    //        {
                                                                                                                                                                    //            var addSliceY = y + sizeDown;
                                                                                                                                                                    //            var removeSliceY = y - sizeUp;
                                                                                                                                                                    //            var realX = y %2 == 0 ? rect.Left : rect.Right-1;

                                                                                                                                                                    //            if (removeSliceY > 0)
                                                                                                                                                                    //            {
                                                                                                                                                                    //                var removeSlice = BuildFromSurface(src, removeSliceY, realX, sizeLeft, sizeRight, 0, 0);
                                                                                                                                                                    //                cube.RemoveSubcube(removeSlice);
                                                                                                                                                                    //            }

                                                                                                                                                                    //            if (addSliceY < rect.Height)
                                                                                                                                                                    //            {
                                                                                                                                                                    //                var addSlice = BuildFromSurface(src, addSliceY, realX, sizeLeft, sizeRight, 0, 0);
                                                                                                                                                                    //                cube.AddSubcube(addSlice);
                                                                                                                                                                    //            }
                                                                                                                                                                    //        }

                                                                                                                                                                    //        for(var x = rect.Left; x < rect.Right; x++)
                                                                                                                                                                    //        {
                                                                                                                                                                    //            var realX = y %2 == 0 ? x : rect.Width-1-x;

                                                                                                                                                                    //            if (cube == null)
                                                                                                                                                                    //            {
                                                                                                                                                                    //                cube = BuildFromSurface(src, realX, y, sizeLeft, sizeRight, sizeUp, sizeDown);
                                                                                                                                                                    //            }
                                                                                                                                                                    //            else
                                                                                                                                                                    //            {
                                                                                                                                                                    //                var addSliceX = y%2 == 0 ? realX + sizeRight : realX - sizeLeft;
                                                                                                                                                                    //                var removeSliceX = y%2 == 0 ? realX - sizeLeft : realX + sizeRight;

                                                                                                                                                                    //                if (removeSliceX > 0 && removeSliceX < rect.Width)
                                                                                                                                                                    //                {
                                                                                                                                                                    //                    var removeSlice = BuildFromSurface(src, removeSliceX, y, 0, 0, sizeUp, sizeDown);
                                                                                                                                                                    //                    cube.RemoveSubcube(removeSlice);
                                                                                                                                                                    //                }

                                                                                                                                                                    //                if (addSliceX > 0 && addSliceX < rect.Width)
                                                                                                                                                                    //                {
                                                                                                                                                                    //                    var addSlice = BuildFromSurface(src, addSliceX, y, 0, 0, sizeUp, sizeDown);
                                                                                                                                                                    //                    cube.AddSubcube(addSlice);
                                                                                                                                                                    //                }

                                                                                                                                                                    //                cube.neighborInfo = GetNeighborInfo(src, realX, y);
                                                                                                                                                                    //            }

                                                                                                                                                                    //            yield return cube;
                                                                                                                                                                    //        }
                                                                                                                                                                    //    }
                                                                                                                                                                    //}
                                                                                                                                                                    //else
                                                                                                                                                                    //{
                                                                                                                                                                    //    for(var x = rect.Left; x < rect.Right; x++)
                                                                                                                                                                    //    for(var y = rect.Top; y < rect.Bottom; y++)
                                                                                                                                                                    //    {
                                                                                                                                                                    //        var realY = x %2 == 0 ? y : rect.Height-1-y;

                                                                                                                                                                    //        yield return BuildFromSurface(src, x, y, sizeLeft, sizeRight, sizeUp, sizeDown);
                                                                                                                                                                    //    }
                                                                                                                                                                    //}

                                                                                                                                                                }

                                                                                                                                                                public void SetCenterToAverage()
                                                                                                                                                                {
                                                                                                                                                                    Center = Average;
                                                                                                                                                                    CenterHsv = HsvColor.FromColor(Average.ToColor());
                                                                                                                                                                }
                                                                                                                                                                public void SetCenterToAverageOpaque()
                                                                                                                                                                {
                                                                                                                                                                    Center = AverageOpaque;
                                                                                                                                                                    CenterHsv = HsvColor.FromColor(AverageOpaque.ToColor());
                                                                                                                                                                }
                                                                                                                                                            }

                                                                                                                                                            public enum AlphaPixelModStyle
                                                                                                                                                            {
                                                                                                                                                                None = 0,
                                                                                                                                                                Debug = 1,
                                                                                                                                                                Alpha_Black = 2,
                                                                                                                                                                Alpha_White = 3,
                                                                                                                                                                Threshold_Midpoint = 4,
                                                                                                                                                                Threshold_Extreme_Opaque = 5,
                                                                                                                                                                Threshold_Extreme_Transparent = 6,
                                                                                                                                                                Threshold_Midpoint_SaveRGB = 7,
                                                                                                                                                                Threshold_Extreme_Opaque_SaveRGB = 8,
                                                                                                                                                                Threshold_Extreme_Transparent_SaveRGB = 9,
                                                                                                                                                                Alpha_To_Grey = 10,
                                                                                                                                                                Alpha_To_Red = 11,
                                                                                                                                                                Alpha_To_Gree = 12,
                                                                                                                                                                Alpha_To_Blue = 13
                                                                                                                                                            }

                                                                                                                                                            unsafe void ProcessSurfaceAlpha(Surface dst, Surface src, params AlphaPixelModStyle[] styles)
                                                                                                                                                            {
                                                                                                                                                                for(var y = 0; y < src.Height; y++)
                                                                                                                                                                {
                                                                                                                                                                    ColorBgra* sPtr = src.GetPointAddressUnchecked(selection.Left, y);

                                                                                                                                                                    for(var x = 0; x < src.Width; x++)
                                                                                                                                                                    {
                                                                                                                                                                        var pixel = *sPtr;

                                                                                                                                                                        foreach(var style in styles)
                                                                                                                                                                        {
                                                                                                                                                                            pixel = ProcessAlphaPixel(pixel, style);
                                                                                                                                                                        }


                                                                                                                                                                        *sPtr = pixel;

                                                                                                                                                                        sPtr++;
                                                                                                                                                                    }
                                                                                                                                                                }
                                                                                                                                                            }
                                                                                                                                                            ColorBgra ProcessAlphaPixel(ColorBgra currentPixel, AlphaPixelModStyle style)
                                                                                                                                                            {
                                                                                                                                                                return ProcessAlphaPixel(currentPixel, (int)style);
                                                                                                                                                            }

                                                                                                                                                            ColorBgra ProcessAlphaPixel(ColorBgra currentPixel, int style)
                                                                                                                                                            {
                                                                                                                                                                switch(style)
                                                                                                                                                                {
                                                                                                                                                                    case 0:
                                                                                                                                                                        return currentPixel;
                                                                                                                                                                        case 1:
                                                                                                                                                                            if (currentPixel == ColorBgra.Transparent) return ColorBgra.White;
                                                                                                                                                                                if (currentPixel == ColorBgra.TransparentBlack) return ColorBgra.Black;
                                                                                                                                                                                HsvColor color =new HsvColor(255-currentPixel.A, 100,100);
                                                                                                                                                                            return ColorBgra.FromColor(color.ToColor());
                                                                                                                                                                            break;
                                                                                                                                                                        case 2:
                                                                                                                                                                            if (currentPixel == ColorBgra.Transparent) { return ColorBgra.TransparentBlack; } else {return currentPixel;}
                                                                                                                                                                            break;
                                                                                                                                                                        case 3:
                                                                                                                                                                            if (currentPixel == ColorBgra.TransparentBlack) { return ColorBgra.Transparent; } else {return currentPixel;}
                                                                                                                                                                            break;
                                                                                                                                                                        case 4:
                                                                                                                                                                            if (currentPixel.A > 127) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 5:
                                                                                                                                                                            if (currentPixel.A > 0) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue,  byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 6:
                                                                                                                                                                            if (currentPixel.A > 254) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 7:
                                                                                                                                                                            if (currentPixel.A > 127) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 8:
                                                                                                                                                                            if (currentPixel.A > 0) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 9:
                                                                                                                                                                            if (currentPixel.A > 254) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
                                                                                                                                                                            break;
                                                                                                                                                                        case 10:
                                                                                                                                                                            return ColorBgra.FromBgra(currentPixel.A,currentPixel.A,currentPixel.A,byte.MaxValue);
                                                                                                                                                                            case 11:
                                                                                                                                                                                return ColorBgra.FromBgra(byte.MinValue,byte.MinValue,currentPixel.A,byte.MaxValue);
                                                                                                                                                                                case 12:
                                                                                                                                                                                    return ColorBgra.FromBgra(byte.MinValue,currentPixel.A,byte.MinValue,byte.MaxValue);
                                                                                                                                                                                    case 13:
                                                                                                                                                                                        return ColorBgra.FromBgra(currentPixel.A,byte.MinValue,byte.MinValue,byte.MaxValue);
                                                                                                                                                                                        default:
                                                                                                                                                                                            throw new NotSupportedException("Can not support alpha modifier option " + style.ToString());
                                                                                                                                                                                        }
                                                                                                                                                                                    }

                                                                                                                                                                                    private ColorBgra RoundEdges(LocalizedPixelCube cube, ColorBgra neutralColor)
                                                                                                                                                                                    {
                                                                                                                                                                                        var hasNeighbor = new Func<ColorBgra?, bool>((color) => !color.HasValue ? false : MatchesColor(color.Value, neutralColor) ? false : true);

                                                                                                                                                                                        var hasNeighborLeft = hasNeighbor(cube.neighborInfo.LeftOne);
                                                                                                                                                                                        var hasNeighborRight = hasNeighbor(cube.neighborInfo.RightOne);
                                                                                                                                                                                        var hasNeighborUp = hasNeighbor(cube.neighborInfo.UpOne);
                                                                                                                                                                                        var hasNeighborDown = hasNeighbor(cube.neighborInfo.DownOne);
                                                                                                                                                                                        var hasNeighborUpperLeft = hasNeighbor(cube.neighborInfo.UpperLeft);
                                                                                                                                                                                        var hasNeighborUpperRight = hasNeighbor(cube.neighborInfo.UpperRight);
                                                                                                                                                                                        var hasNeighborLowerLeft = hasNeighbor(cube.neighborInfo.LowerLeft);
                                                                                                                                                                                        var hasNeighborLowerRight = hasNeighbor(cube.neighborInfo.LowerRight);

                                                                                                                                                                                        var perpendicularSum = 0;
                                                                                                                                                                                        perpendicularSum += (hasNeighborLeft ? 1 : 0);
                                                                                                                                                                                        perpendicularSum += (hasNeighborRight ? 1 : 0);
                                                                                                                                                                                        perpendicularSum += (hasNeighborUp ? 1 : 0);
                                                                                                                                                                                        perpendicularSum += (hasNeighborDown ? 1 : 0);

                                                                                                                                                                                        var diagonalSum = 0;
                                                                                                                                                                                        diagonalSum += (hasNeighborUpperLeft ? 1 : 0);
                                                                                                                                                                                        diagonalSum += (hasNeighborUpperRight ? 1 : 0);
                                                                                                                                                                                        diagonalSum += (hasNeighborLowerLeft ? 1 : 0);
                                                                                                                                                                                        diagonalSum += (hasNeighborLowerRight ? 1 : 0);

                                                                                                                                                                                        if (MatchesColor(cube.Center, neutralColor))
                                                                                                                                                                                        {
                                                                                                                                                                                            return neutralColor;
                                                                                                                                                                                        }

                                                                                                                                                                                        if (perpendicularSum == 0 || perpendicularSum == 1)
                                                                                                                                                                                        {
                                                                                                                                                                                            return neutralColor;
                                                                                                                                                                                        }
                                                                                                                                                                                        else if (perpendicularSum == 2)
                                                                                                                                                                                        {
                                                                                                                                                                                            if (hasNeighborLeft && hasNeighborUp)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborUpperLeft)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return neutralColor;
                                                                                                                                                                                                }

                                                                                                                                                                                                if (!hasNeighborUpperRight && !hasNeighborLowerLeft)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborRight && hasNeighborUp)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborUpperRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return neutralColor;
                                                                                                                                                                                                }

                                                                                                                                                                                                if (!hasNeighborUpperLeft && !hasNeighborLowerRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborLeft && hasNeighborDown)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborLowerLeft)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }

                                                                                                                                                                                                if (!hasNeighborUpperLeft && !hasNeighborLowerRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborRight && hasNeighborDown)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborLowerRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }

                                                                                                                                                                                                if (!hasNeighborLowerLeft && !hasNeighborUpperRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if ((hasNeighborLeft && hasNeighborRight) || (hasNeighborUp && hasNeighborDown))
                                                                                                                                                                                            {
                                                                                                                                                                                                return  neutralColor;
                                                                                                                                                                                            }
                                                                                                                                                                                        }
                                                                                                                                                                                        else if (perpendicularSum == 3)
                                                                                                                                                                                        {
                                                                                                                                                                                            if (hasNeighborUp && hasNeighborDown && hasNeighborLeft)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborUpperLeft && !hasNeighborLowerLeft)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborUp && hasNeighborDown && hasNeighborRight)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborUpperRight && !hasNeighborLowerLeft)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborLeft && hasNeighborRight && hasNeighborUp)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborUpperLeft && !hasNeighborUpperRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                            else if (hasNeighborLeft && hasNeighborRight && hasNeighborDown)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (!hasNeighborLowerLeft && !hasNeighborLowerRight)
                                                                                                                                                                                                {
                                                                                                                                                                                                    return  neutralColor;
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                        }
                                                                                                                                                                                        else if (perpendicularSum == 4)
                                                                                                                                                                                        {
                                                                                                                                                                                            if (diagonalSum == 0)
                                                                                                                                                                                            {
                                                                                                                                                                                                return  neutralColor;
                                                                                                                                                                                            }
                                                                                                                                                                                        }

                                                                                                                                                                                        return cube.Center;
                                                                                                                                                                                    }

                                                                                                                                                                                    private bool MatchesColor(ColorBgra currentColor, ColorBgra neutralColor)
                                                                                                                                                                                    {
                                                                                                                                                                                        if (neutralColor == ColorBgra.Transparent || neutralColor == ColorBgra.TransparentBlack)
                                                                                                                                                                                        {
                                                                                                                                                                                            if (currentColor.A == byte.MinValue)
                                                                                                                                                                                            {
                                                                                                                                                                                                return true;
                                                                                                                                                                                            }
                                                                                                                                                                                        }

                                                                                                                                                                                        if (currentColor == neutralColor)
                                                                                                                                                                                        {
                                                                                                                                                                                            return true;
                                                                                                                                                                                        }

                                                                                                                                                                                        return false;
                                                                                                                                                                                    }

                                                                                                                                                                                    unsafe void RemoveIslandsFromSurface(Surface dst, Surface src)
                                                                                                                                                                                    {
                                                                                                                                                                                        var pixelsToRemove = new Stack<Pair<int,int>>();

                                                                                                                                                                                        var temp = new Surface(src.Size);
                                                                                                                                                                                        temp.CopySurface(src);
                                                                                                                                                                                        dst.CopySurface(src);

                                                                                                                                                                                        for (int y = selection.Top; y < selection.Bottom; y++)
                                                                                                                                                                                        {
                                                                                                                                                                                            ColorBgra* wPtr = temp.GetPointAddressUnchecked(selection.Left, y);

                                                                                                                                                                                            for (int x = selection.Left; x < selection.Right; x++)
                                                                                                                                                                                            {
                                                                                                                                                                                                if (IsCancelRequested) return;

                                                                                                                                                                                            var stack = new Stack<Pair<int,int>>();
                                                                                                                                                                                            var count = 0;

                                                                                                                                                                                            FloodFill(x, y, temp, selection,
                                                                                                                                                                                            (colorSet) =>
                                                                                                                                                                                            {
                                                                                                                                                                                                count = colorSet.floodedPixels;

                                                                                                                                                                                                if (MatchesColor(colorSet.currentColor, ColorBgra.TransparentBlack))
                                                                                                                                                                                                {
                                                                                                                                                                                                    return false;
                                                                                                                                                                                                }

                                                                                                                                                                                                stack.Push(colorSet.currentCoordinates);
                                                                                                                                                                                                return true;
                                                                                                                                                                                                },
                                                                                                                                                                                                (colorSet) => ColorBgra.TransparentBlack
                                                                                                                                                                                                );

                                                                                                                                                                                                if (count > 0 && count < 4*4)
                                                                                                                                                                                                {
                                                                                                                                                                                                    while(stack.Count > 0)
                                                                                                                                                                                                    {
                                                                                                                                                                                                        if (IsCancelRequested) return;
                                                                                                                                                                                                    pixelsToRemove.Push(stack.Pop());
                                                                                                                                                                                                }
                                                                                                                                                                                            }

                                                                                                                                                                                            wPtr++;
                                                                                                                                                                                        }
                                                                                                                                                                                    }

                                                                                                                                                                                    if (IsCancelRequested) return;

                                                                                                                                                                                var searchPixels = new Dictionary<ColorBgra, List<Pair<int,int>>>();

                                                                                                                                                                                while(pixelsToRemove.Count > 0)
                                                                                                                                                                                {
                                                                                                                                                                                    if (IsCancelRequested) return;

                                                                                                                                                                                var coord = pixelsToRemove.Pop();

                                                                                                                                                                                dst[coord.First, coord.Second] = ColorBgra.TransparentBlack;
                                                                                                                                                                            }
                                                                                                                                                                        }



                                                                                                                                                                        private void RareLog(int x, int y, int howFrequent, string formatString, params object[] args)
                                                                                                                                                                        {
                                                                                                                                                                            var random = rng.Next(0, (int)(width*height));

                                                                                                                                                                            if (howFrequent > 1000)
                                                                                                                                                                            {
                                                                                                                                                                                howFrequent = 1000;
                                                                                                                                                                            }

                                                                                                                                                                            if (random < howFrequent)
                                                                                                                                                                            {
                                                                                                                                                                                var prefix = "[" + x.ToString() + ", " + y.ToString() + "]:  ";
                                                                                                                                                                                var message = prefix + string.Format(formatString, args);

                                                                                                                                                                                Debug.WriteLine(message);
                                                                                                                                                                            }
                                                                                                                                                                        }

                                                                                                                                                                        private void ColorSwap(Surface dst, Surface src, ColorBgra remove, ColorBgra add)
                                                                                                                                                                        {
                                                                                                                                                                            for(var y = 0; y < src.Height; y++)
                                                                                                                                                                            {
                                                                                                                                                                                for(var x = 0; x < src.Width; x++)
                                                                                                                                                                                {
                                                                                                                                                                                    if (src[x,y] == remove)
                                                                                                                                                                                    {
                                                                                                                                                                                        dst[x,y] = add;
                                                                                                                                                                                    }
                                                                                                                                                                                    else
                                                                                                                                                                                    {
                                                                                                                                                                                        dst[x,y] = src[x,y];
                                                                                                                                                                                    }
                                                                                                                                                                                }
                                                                                                                                                                            }
                                                                                                                                                                        }