using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AudioRecorder.Core
{
    public class WaveFormVisual : FrameworkElement, IWaveFormRenderer
    {
        private VisualCollection visualCollection;
        private double yTranslate = 40;
        private double yScale = 40;
        private double xSpacing = 2;
        private List<float> maxValues;
        private List<float> minValues;

        public WaveFormVisual()
        {
            this.visualCollection = new VisualCollection(this);
            this.Reset();
            this.SizeChanged += WaveFormVisual_SizeChanged;
        }

        private void WaveFormVisual_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double padding = e.NewSize.Height / 10; // 10 percent padding
            this.yScale = (e.NewSize.Height - padding) / 2;
            this.yTranslate = padding + yScale;
            this.Redraw();
        }

        public double XSpacing => this.XSpacing;

        private DrawingVisual CreateWaveFormVisual()
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // retrieve the drawingcontext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            if (maxValues.Count > 0)
            {
                this.RenderPolygon(drawingContext);
            }

            // persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        private void RenderPolygon(DrawingContext drawingContext)
        {
            var fillBrush = Brushes.Bisque;
            var borderPen = new Pen(Brushes.Black, 1.0);

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = this.CreatePoint(this.maxValues, 0);

            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

            for (int i = 1; i < this.maxValues.Count; i++)
            {
                myPathSegmentCollection.Add(new LineSegment(this.CreatePoint(this.maxValues, i), true));
            }

            for (int i = this.minValues.Count - 1; i >= 0; i--)
            {
                myPathSegmentCollection.Add(new LineSegment(this.CreatePoint(this.minValues, i), true));
            }

            myPathFigure.Segments = myPathSegmentCollection;
            PathGeometry myPathGeometry = new PathGeometry();

            myPathGeometry.Figures.Add(myPathFigure);

            drawingContext.DrawGeometry(fillBrush, borderPen, myPathGeometry);
        }

        private Point CreatePoint(List<float> values, int xpos)
        {
            return new Point(xpos * this.xSpacing, this.SampleToYPosition(values[xpos]));
        }

        // provide a required override for the VisualChildrenCount proprety.
        protected override int VisualChildrenCount => this.visualCollection.Count;

        // provide a required override for the getvisualchild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= this.visualCollection.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return this.visualCollection[index];
        }

        public void AddValue(float maxValue, float minValue)
        {
            this.maxValues.Add(maxValue);
            this.minValues.Add(minValue);
            this.Redraw();
        }

        private void Redraw()
        {
            this.visualCollection.Clear();
            this.visualCollection.Add(CreateWaveFormVisual());
            this.InvalidateVisual();
        }

        private double SampleToYPosition(float value)
        {
            return this.yTranslate + value * this.yScale;
        }

        public void Reset()
        {
            this.maxValues = new List<float>();
            this.minValues = new List<float>();
            this.visualCollection.Clear();
            this.visualCollection.Add(CreateWaveFormVisual());
        }
    }
}