using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FCSChart.Graphical
{
    /// <summary>
    /// 四边形
    /// </summary>
    public class QuadrilateralGraphical : PolygonGraphical
    {

        public QuadrilateralGraphical() : base() { }
        protected override void CreateNewMode()
        {
            GraphicalModel = new QuadrilateralGraphicalModel()
            {
                Points = new ObservableCollection<Point>(),
                Name = CreateNewGraphicalNameFaction?.Invoke(this)
            };
        }
        public QuadrilateralGraphical(QuadrilateralGraphicalModel model) : base(model) { }

        #region function
        internal override void PanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is QuadrilateralGraphicalModel model && model.Points.Count <= 0)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                var y = OwnerChart.YAxis.GetLocationValue(point.Y);
                var tempPoint = new Point(x, y);
                model.Points.Add(tempPoint);
                model.Points.Add(tempPoint);
                model.Points.Add(tempPoint);
                model.Points.Add(tempPoint);
            }
        }
        internal override void PanelMouseMove(object sender, MouseEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is QuadrilateralGraphicalModel model && model.Points.Count == 4)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                var y = OwnerChart.YAxis.GetLocationValue(point.Y);
                model.Points[2] = new Point(x, y);
                model.Points[1] = new Point(x, model.Points[0].Y);
                model.Points[3] = new Point(model.Points[0].X, y);
                Drawing();
            }
        }
        internal override void PanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsCreateing && GraphicalModel is QuadrilateralGraphicalModel model)
            {
                if (model.Points[0] == model.Points[2]) return;
                Drawing();
                IsCreateing = false;
            }
        }

        protected override void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Path path && path.Data is EllipseGeometry ellipse && GraphicalModel is QuadrilateralGraphicalModel model)
            {
                var point = e.GetPosition(OwnerChart.ViewPanel);
                var index = ControlShapes.IndexOf(path);
                var value = GraphicalPointConverter.ConvertBack(point, null, OwnerChart, null);
                if (value is Point p)
                {
                    model.Points[index] = p;
                    switch (index)
                    {
                        case 0:
                            model.Points[1] = new Point(model.Points[1].X, p.Y);
                            model.Points[3] = new Point(p.X, model.Points[3].Y);
                            break;
                        case 1:
                            model.Points[0] = new Point(model.Points[0].X, p.Y);
                            model.Points[2] = new Point(p.X, model.Points[2].Y);
                            break;
                        case 2:
                            model.Points[1] = new Point(p.X, model.Points[1].Y);
                            model.Points[3] = new Point(model.Points[3].X, p.Y);
                            break;
                        case 3:
                            model.Points[0] = new Point(p.X, model.Points[0].Y);
                            model.Points[2] = new Point(model.Points[2].X, p.Y);
                            break;
                        default:
                            break;
                    }

                    Drawing();
                }
                ellipse.Center = point;
                e.Handled = true;
            }
        }
        #endregion
        public override void RefreshSubset()
        {
            if (!IsCreateing && GraphicalModel is QuadrilateralGraphicalModel model && OwnerChart != null && OwnerChart.ItemsSource != null)
            {
                var minX = model.Points.Min(p => p.X);
                var maxX = model.Points.Max(p => p.X);
                var minY = model.Points.Min(p => p.Y);
                var maxY = model.Points.Max(p => p.Y);
                var indexX = OwnerChart.X;
                var indexY = OwnerChart.Y;
                var items = OwnerChart.ItemsSource;
                Subset = items.Where(p => Convert.ToDouble(p[indexX]) >= minX && Convert.ToDouble(p[indexX]) <= maxX && Convert.ToDouble(p[indexY]) >= minY && Convert.ToDouble(p[indexY]) <= maxY).ToList();
            }
        }
    }

    /// <summary>
    /// 四边形数据
    /// </summary>
    public class QuadrilateralGraphicalModel : PolygonGraphicalModel
    {

    }
}
