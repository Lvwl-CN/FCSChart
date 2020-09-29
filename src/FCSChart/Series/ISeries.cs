using System;
using System.Windows;
using System.Windows.Media;

namespace FCSChart.Series
{
    public abstract class ISeries : FrameworkElement
    {
        #region property
        #region 图形外观
        /// <summary>
        /// 填充色
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(ISeries), new PropertyMetadata(new SolidColorBrush(new Color() { A = 0xff, R = 0x40, G = 0x90, B = 0x00 })));

        /// <summary>
        /// 边框色
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(ISeries), new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// 边框宽度
        /// </summary>
        public Thickness StrokeThickness
        {
            get { return (Thickness)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(Thickness), typeof(ISeries), new PropertyMetadata(default(Thickness)));
        #endregion

        /// <summary>
        /// 父容器
        /// </summary>
        public Chart OwnerChart { get; internal set; }
        protected readonly DrawingVisual DV;
        /// <summary>
        /// 获取渐变颜色的方法
        /// </summary>
        public Func<long, long, Color> GetGradientColor { get; set; }
        #region 变形--用于缩放和移动，缩放图形容易变形，所以缩放改为重绘图形来实现
        Matrix Matrix;
        MatrixTransform MatrixTransform { get; set; }
        #endregion
        #endregion
        public ISeries()
        {
            GetGradientColor = Helper.GetGradientColor;
            DV = new DrawingVisual();
            this.AddVisualChild(DV);

            this.RenderTransform = MatrixTransform = new MatrixTransform(Matrix);
        }

        #region event
        /// <summary>
        /// 某些依赖属性改变后，需要重新绘制图形
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        protected static void NeedRedrawingProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ISeries temp) temp.Drawing();
        }
        #endregion
        #region function
        /// <summary>
        /// 绘制图表界面
        /// </summary>
        /// <param name="items"></param>
        internal abstract void Drawing();


        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal virtual void Move(double x, double y)
        {
            Matrix.OffsetX += x;
            Matrix.OffsetY += y;
            MatrixTransform.Matrix = Matrix;
        }
        /// <summary>
        /// 缩放图形
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        internal virtual void Zoom(Point center, double scaleX, double scaleY)
        {
            Point pointToContent = this.RenderTransform.Inverse.Transform(center);
            Matrix.M11 *= scaleX;
            Matrix.M22 *= scaleY;

            Matrix.OffsetX = -((pointToContent.X * Matrix.M11) - center.X);
            Matrix.OffsetY = -((pointToContent.Y * Matrix.M22) - center.Y);
            MatrixTransform.Matrix = Matrix;
        }
        /// <summary>
        /// 清除图形转换
        /// </summary>
        internal virtual void ClearTransform()
        {
            Matrix.M11 = Matrix.M22 = 1;
            Matrix.OffsetX = Matrix.OffsetY = 0;
            MatrixTransform.Matrix = Matrix;
        }
        #endregion

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            return DV;
        }
    }
}
