using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ba2Explorer.Controls
{
    /* TODO:
     * - make use of Control.FontSize
     * - make use of Control.FontFamily
     * - fix vert scrolling with scrollbar, it doesn't capture last 2 lines (somewhat fixed)
     * - fix vert scrolling with mouse wheel that goes down the lines
     * - fix horizontal scrolling with scrollbar when resizing window 
     * - fix bunch of TODOs in code.
     * - reduce memory allocation for each redraw, cache GlyphRuns.
    */ 

    /// <summary>
    /// TextBlock (actually closer to Label) that doesn't render invisible lines and implements custom scrolling behavior.
    /// Lacks gazillions of features, use only for performance reasons (Text.Length > 1000).
    /// </summary>
    public class FastTextBlock : Control, IScrollInfo
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FastTextBlock));

        /// <summary>
        /// Gets or sets text that will be rendered in text block. Causes visual invalidation.
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set {
                SetValue(TextProperty, value);
                SetDirty();

                m_maxLineWidth = 0.0d;
                if (value != null || value.Length != 0)
                    m_linesIndices = GetLinesIndices();
                else
                    m_linesIndices = null;
            }
        }

        /// <summary>
        /// Marks visual as dirty and invalidates it.
        /// </summary>
        private void SetDirty()
        {
            m_dirty = true;
            InvalidateVisual();
        }

        /// <summary>
        /// Indicates whether text surface should be redrawn.
        /// </summary>
        private bool m_dirty = true;

        private GlyphTypeface m_glyphTypeface;

        /// <summary>
        /// Line height coefficent. To get DPI height for line, use GetLineHeightDPI().
        /// </summary>
        private double m_lineHeightCoeff;

        /// <summary>
        /// List of GlyphRuns to render.
        /// </summary>
        private List<GlyphRun> m_runs;

        /// <summary>
        /// Height of drawn lines (GlyphRuns)
        /// </summary>
        private double m_drawnLinesHeight;

        /// <summary>
        /// Font size in ems. TODO: use Control.FontSize
        /// </summary>
        private const int m_fontSize = 12;

        /// <summary>
        /// Represents glyph that will be rendered if some character has no glyph bound to it.
        /// </summary>
        private ushort m_unknownGlyphIndex;

        /// <summary>
        /// List of tuple that contain start and end character indices for each line in string `Text`.
        /// </summary>
        private List<Tuple<int, int>> m_linesIndices;

        /// <summary>
        /// Max line width detected in `m_linesIndices`. This is not width in characters, but width in DPI units.
        /// </summary>
        private double m_maxLineWidth;

        /// <summary>
        /// Creates new instance of FastTextBlock. Attach to some parent control before setting `Text` property,
        /// unless you want bunch of errors in your face.
        /// </summary>
        public FastTextBlock()
        {
            // TODO: use Control.FontFamily.
            var fontFamily = new FontFamily("Consolas");
            m_lineHeightCoeff = fontFamily.LineSpacing;

            var typeface = new Typeface(
                fontFamily,
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Normal);

            if (!typeface.TryGetGlyphTypeface(out m_glyphTypeface))
                throw new Exception("TODO");

            m_unknownGlyphIndex = m_glyphTypeface.CharacterToGlyphMap['?'];
            m_runs = new List<GlyphRun>();

            // TODO: remove listener
            DependencyPropertyDescriptor
                .FromProperty(Control.FontSizeProperty, typeof(Control))
                .AddValueChanged(this, FontSizePropertyChanged);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // If new size is bigger than last size, then visible area increases, so redraw required.
            if (sizeInfo.NewSize.Width > sizeInfo.PreviousSize.Width ||
                sizeInfo.NewSize.Height > sizeInfo.PreviousSize.Height)
            {
                SetDirty();
            }
        }

        private void FontSizePropertyChanged(object sender, EventArgs e)
        {
            m_dirty = true;

            // TODO

            InvalidateVisual();
        }


        /// <summary>
        /// Returns list of tuples that contain start and end indices of `Text` property.
        /// </summary>
        private List<Tuple<int, int>> GetLinesIndices()
        {
            List<Tuple<int, int>> lines = new List<Tuple<int, int>>();
            int lineEndIndex = 0;
            int lineStartIndex = 0;
            for (int i = 0; i < Text.Length; ++i)
            {

                char c = Text[i];
                if (c == '\r' && (i + 1) < Text.Length && Text[i + 1] == '\n')
                {
                    lines.Add(new Tuple<int, int>(lineStartIndex, lineEndIndex));
                    i += 1; // skip \n
                    lineStartIndex = i + 1;
                    lineEndIndex = lineStartIndex;
                    continue;
                }
                ++lineEndIndex;
            }
            if (lineEndIndex != lineStartIndex)
                lines.Add(new Tuple<int, int>(lineStartIndex, lineEndIndex));
            return lines;
        }

        /// <summary>
        /// Creates new GlyphRun instance for line in Text property and adds it to list of GlyphRuns if line has non-zero length 
        /// (doesn't add GlyphRun otherwise). Also changes `m_maxLineWidth` if required. Takes `m_horizontalOffset` in account.
        /// </summary>
        private void AddGlyphRunForLine(int line, List<GlyphRun> runs)
        {
            var startEndIndices = m_linesIndices[line];
            int lineStartIndex = startEndIndices.Item1;
            int lineEndIndex = startEndIndices.Item2;
            int lineLength = lineEndIndex - lineStartIndex;
            if (lineLength == 0)
            {
                m_drawnLinesHeight += GetLineHeightDPI();
                return;
            }

            ushort[] indices = new ushort[lineLength];
            double[] advanceWidths = new double[lineLength];

            int textOffset = lineStartIndex;
            double lineWidth = 0.0d;
            for (int i = 0; i < lineLength; ++i)
            {
                char c = Text[textOffset];
                ushort glyphIndex;
                if (!m_glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out glyphIndex))
                    glyphIndex = m_unknownGlyphIndex;
                indices[i] = glyphIndex;
                advanceWidths[i] = m_glyphTypeface.AdvanceWidths[glyphIndex] * m_fontSize;
                lineWidth += advanceWidths[i];
                ++textOffset;
            }
            if (lineWidth > m_maxLineWidth)
                m_maxLineWidth = lineWidth;

            var run = new GlyphRun(m_glyphTypeface, 0, false, m_fontSize, indices, new Point(Padding.Left - m_horizontalOffset,
                m_drawnLinesHeight + Padding.Top + (GetLineHeightDPI())),
                advanceWidths, null, null, null, null, null, null);

            m_drawnLinesHeight += GetLineHeightDPI();

            runs.Add(run);
        }

        private double GetLineHeightDPI() => m_lineHeightCoeff * m_fontSize;

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(ViewportWidth, ViewportHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Text == null || Text.Length == 0) return;

            //Debug.WriteLine(m_dirty ? "dirty" : "not dirty");
            if (m_dirty)
            {
                m_drawnLinesHeight = 0.0d;

                int totalLines = m_linesIndices.Count;
                // adding 2 to totalLines to fix weird issue with scrolling vertical scrollbar that doesn't capture last 2 lines.
                m_extentHeight = (2 + totalLines) * (GetLineHeightDPI());
                if (m_verticalOffset > m_extentHeight)
                    m_verticalOffset = m_extentHeight;

                double visibleWidth  = (double)Parent.GetValue(ActualWidthProperty);
                double visibleHeight = (double)Parent.GetValue(ActualHeightProperty);

                m_viewportWidth = visibleWidth;
                m_viewportHeight = visibleHeight;

                int firstLineToDraw = (int)(m_verticalOffset / (GetLineHeightDPI()));
                firstLineToDraw = MathHelper.Clamp(firstLineToDraw, 0, totalLines);

                // amount of lines that are visible in viewport
                int lastLineToDraw = firstLineToDraw + 1 + (int)(visibleHeight / (GetLineHeightDPI()));
                lastLineToDraw = MathHelper.Clamp(lastLineToDraw, 0, totalLines);

                m_runs.Clear();
                for (int line = firstLineToDraw; line < lastLineToDraw; ++line)
                {
                    AddGlyphRunForLine(line, m_runs);
                }

                // placing it there because m_maxLineWidth is fetched in AddGlyphRunForLine
                m_extentWidth = m_maxLineWidth;
                if (m_horizontalOffset > m_extentWidth)
                    m_horizontalOffset = m_extentWidth;

                m_canHorizontallyScroll = visibleWidth < m_extentWidth; 
                m_canVerticallyScroll = visibleHeight < m_extentHeight;

                m_scroll.InvalidateScrollInfo();

                m_dirty = false;
            }

            foreach (var run in m_runs) {
                drawingContext.DrawGlyphRun(Brushes.Black, run);
            }
        }

        #region IScrollInfo

        private bool m_canVerticallyScroll;
        public bool CanVerticallyScroll
        {
            get { return m_canVerticallyScroll; }
            set { m_canVerticallyScroll = true; }
        }

        private bool m_canHorizontallyScroll;
        public bool CanHorizontallyScroll
        {
            get { return m_canHorizontallyScroll; }
            set { m_canHorizontallyScroll = true; }
        }

        private double m_extentWidth;
        public double ExtentWidth => m_extentWidth;

        private double m_extentHeight;
        public double ExtentHeight => m_extentHeight;

        private double m_viewportWidth;
        public double ViewportWidth => m_viewportWidth;

        private double m_viewportHeight;
        public double ViewportHeight => m_viewportHeight;

        private double m_horizontalOffset;
        public double HorizontalOffset => m_horizontalOffset;

        private double m_verticalOffset;
        public double VerticalOffset => m_verticalOffset;

        private ScrollViewer m_scroll;
        public ScrollViewer ScrollOwner
        {
            get { return m_scroll; }
            set { m_scroll = value; }
        }

        private void AddHorizontalOffset(double add)
        {
            SetHorizontalOffset(add + m_horizontalOffset);
        }

        private void AddVerticalOffset(double add)
        {
            SetVerticalOffset(add + m_verticalOffset);
        }

        public void LineUp() => AddVerticalOffset(-GetLineHeightDPI());
        public void LineDown() => AddVerticalOffset(GetLineHeightDPI());
        public void LineLeft() => AddHorizontalOffset(-10);
        public void LineRight() => AddHorizontalOffset(10);

        public void PageUp() => AddVerticalOffset(-RenderSize.Height);
        public void PageDown() => AddVerticalOffset(RenderSize.Height);
        public void PageLeft() => AddHorizontalOffset(-100);
        public void PageRight() => AddHorizontalOffset(100);

        public void MouseWheelUp() => AddVerticalOffset(-GetLineHeightDPI() * 3);
        public void MouseWheelDown() => AddVerticalOffset(GetLineHeightDPI() * 3);
        public void MouseWheelLeft() => AddHorizontalOffset(-10);
        public void MouseWheelRight() => AddHorizontalOffset(10);

        public void SetHorizontalOffset(double offset)
        {
            m_horizontalOffset = MathHelper.Clamp(offset, 0, m_extentWidth); // Height
            SetDirty();

            //Debug.WriteLine("SetHorizontalOffset {0}", m_horizontalOffset);
        }

        public void SetVerticalOffset(double offset)
        {
            m_verticalOffset = MathHelper.Clamp(offset, 0, m_extentHeight); // Width
            SetDirty();

            //Debug.WriteLine("SetVerticalOffset {0}", m_verticalOffset);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            //Debug.WriteLine("MakeVisible {0} {1}", visual, rectangle);

            return rectangle;
        }

        #endregion

    }
}
