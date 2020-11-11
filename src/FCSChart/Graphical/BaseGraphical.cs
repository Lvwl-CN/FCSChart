using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FCSChart.Graphical
{
    public abstract class BaseGraphical : NotifyPropertyChanged, IDisposable
    {
        #region converter
        /// <summary>
        /// 点位集合坐标值转化实际显示位置
        /// </summary>
        internal static readonly Converters.GraphicalPointsToViewValueConverter GraphicalPointsConverter = new Converters.GraphicalPointsToViewValueConverter();
        /// <summary>
        /// 点位坐标值转化实际显示位置
        /// </summary>
        internal static readonly Converters.GraphicalPointToViewValueConverter GraphicalPointConverter = new Converters.GraphicalPointToViewValueConverter();
        /// <summary>
        /// bool转化为visibility值
        /// </summary>
        internal static readonly Converters.BoolToVisibilityConverter BoolVisibilityConverter = new Converters.BoolToVisibilityConverter();
        #endregion
        public Func<BaseGraphical, string> CreateNewGraphicalNameFaction { get; set; } = Helper.CreateNewGraphicalNameFaction;
        #region property
        /// <summary>
        /// 父容器
        /// </summary>
        public ChartWithGraphicals OwnerChart { get; internal set; }

        private BaseGraphicalModel graphicalModel;
        /// <summary>
        /// 数据模型
        /// </summary>
        public BaseGraphicalModel GraphicalModel
        {
            get { return graphicalModel; }
            protected set
            {
                graphicalModel = value;
                if (value != null) MonitorControlData(value);
            }
        }

        private bool isCreateing = false;
        /// <summary>
        /// 是否正在创建
        /// </summary>
        public bool IsCreateing
        {
            get { return isCreateing; }
            protected set
            {
                isCreateing = value;
                OnPropertyChanged("IsCreateing");
                if (!value)
                {
                    DrawingControl();
                    RefreshSubset();
                }
            }
        }

        #endregion

        public BaseGraphical() { IsCreateing = true; CreateNewMode(); }
        /// <summary>
        /// 创建新的门数据模型
        /// </summary>
        protected abstract void CreateNewMode();

        public BaseGraphical(BaseGraphicalModel model)
        {
            this.GraphicalModel = model;
        }

        #region 容器鼠标事件-用户门的创建
        /// <summary>
        /// 容器鼠标按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal abstract void PanelMouseDown(object sender, MouseButtonEventArgs e);
        /// <summary>
        /// 容器鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal abstract void PanelMouseMove(object sender, MouseEventArgs e);
        /// <summary>
        /// 容器鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal abstract void PanelMouseUp(object sender, MouseButtonEventArgs e);

        #endregion

        #region 变形--用于缩放和移动
        /// <summary>
        /// 移动门
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal abstract void Move(double x, double y);
        ///// <summary>
        ///// 缩放门
        ///// </summary>
        ///// <param name="percent">缩放比例</param>
        ///// <param name="center">缩放中心点位</param>
        //public abstract void Zoom(double percent, Point center);
        ///// <summary>
        ///// 旋转门
        ///// </summary>
        ///// <param name="angle"></param>
        ///// <param name="center"></param>
        //public abstract void Rotate(double angle, Point center);
        #endregion

        /// <summary>
        /// 清理资源--删除图形时可调用
        /// </summary>
        public virtual void Dispose()
        {
            if (OwnerChart != null && OwnerChart.ViewPanel != null)
            {
                if (GraphicalShape != null && OwnerChart.ViewPanel.Children.Contains(GraphicalShape))
                    OwnerChart.ViewPanel.Children.Remove(GraphicalShape);
                if (ControlShapes != null && ControlShapes.Count > 0)
                {
                    foreach (var shape in ControlShapes)
                    {
                        if (OwnerChart.ViewPanel.Children.Contains(shape))
                            OwnerChart.ViewPanel.Children.Remove(shape);
                    }
                }
                OwnerChart.Graphicals.Remove(this);
            }
        }

        #region 门显示相关
        private ContextMenu contextMenu;
        /// <summary>
        /// 图形右键菜单
        /// </summary>
        public ContextMenu ContextMenu
        {
            get
            {
                if (contextMenu == null)
                {
                    contextMenu = new ContextMenu();
                    var menuitem = new MenuItem() { Header = "Remove" };
                    menuitem.Click += (sender, e) => this.Dispose();
                    contextMenu.Items.Add(menuitem);
                }
                return contextMenu;
            }
            set { contextMenu = value; OnPropertyChanged("ContextMenu"); }
        }

        private Shape graphicalShape;
        /// <summary>
        /// 门的显示形状
        /// </summary>
        protected Shape GraphicalShape
        {
            get { return graphicalShape; }
            set
            {
                graphicalShape = value;
                if (value != null)
                {
                    value.SetBinding(Shape.FillProperty, new Binding("Fill") { Source = this });
                    value.SetBinding(Shape.StrokeProperty, new Binding("Stroke") { Source = this });
                    value.SetBinding(Shape.StrokeThicknessProperty, new Binding("StrokeThickness") { Source = this });
                    value.SetBinding(FrameworkElement.ToolTipProperty, new Binding("Name") { Source = GraphicalModel });
                    value.SetBinding(FrameworkElement.ContextMenuProperty, new Binding("ContextMenu") { Source = this });
                    value.Focusable = true;
                    value.MouseDown += Graphical_MouseDown;
                    value.MouseMove += Graphical_MouseMove;
                    value.MouseLeftButtonUp += (sender, e) => { RefreshSubset(); };
                    value.IsKeyboardFocusedChanged += (sender, e) => { this.DrawingControl(); };
                    value.KeyDown += (sender, e) =>
                    {
                        if (e.Key == Key.Left)
                        {
                            Move(-1, 0); e.Handled = true;
                        }
                        else if (e.Key == Key.Right)
                        {
                            Move(1, 0); e.Handled = true;
                        }
                        else if (e.Key == Key.Up)
                        {
                            Move(0, -1); e.Handled = true;
                        }
                        else if (e.Key == Key.Down)
                        {
                            Move(0, 1); e.Handled = true;
                        }

                    };
                    OwnerChart.ViewPanel.Children.Add(value);
                }
            }
        }

        private Brush fill;
        /// <summary>
        /// 填充色
        /// </summary>
        public Brush Fill
        {
            get
            {
                if (fill == null)
                {
                    TextBlock textBlock = new TextBlock() { Foreground = new SolidColorBrush(new Color() { A = 0x20, R = 0x00, G = 0x00, B = 0x00 }), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    textBlock.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = GraphicalModel });
                    Border border = new Border() { Background = Helper.GetNewSolidColorBrush(0x20), Child = textBlock, Width = 50, Height = 50 };
                    fill = new VisualBrush(border);
                }
                return fill;
            }
            set { fill = value; OnPropertyChanged("Fill"); }
        }
        private Brush stroke = Brushes.Black;
        /// <summary>
        /// 边框色
        /// </summary>
        public Brush Stroke
        {
            get { return stroke; }
            set { stroke = value; OnPropertyChanged("Stroke"); }
        }
        private double strokeThickness = 1;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public double StrokeThickness
        {
            get { return strokeThickness; }
            set { strokeThickness = value; OnPropertyChanged("StrokeThickness"); }
        }

        /// <summary>
        /// 绘制门
        /// </summary>
        public abstract void Drawing();

        #region 门事件--移动，选中
        protected Point GraphicalMoveStartPoint { get; set; }
        private void Graphical_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GraphicalMoveStartPoint = e.GetPosition(OwnerChart.ViewPanel);
            GraphicalShape.Focus();
            e.Handled = true;
        }
        private void Graphical_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (GraphicalMoveStartPoint != default)
                {
                    var point = e.GetPosition(OwnerChart.ViewPanel);
                    var x = point.X - GraphicalMoveStartPoint.X;
                    var y = point.Y - GraphicalMoveStartPoint.Y;
                    GraphicalMoveStartPoint = point;
                    Move(x, y);
                    e.Handled = true;
                }
            }
        }
        #endregion
        #endregion

        #region 门控制
        private ObservableCollection<Shape> controlShapes;
        /// <summary>
        /// 控制图形的点位图形
        /// </summary>
        protected ObservableCollection<Shape> ControlShapes
        {
            get
            {
                if (controlShapes == null)
                {
                    controlShapes = new ObservableCollection<Shape>();
                    controlShapes.CollectionChanged += (sender, e) =>
                    {
                        switch (e.Action)
                        {
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                                foreach (var item in e.NewItems)
                                {
                                    if (item is Shape shape)
                                    {
                                        shape.Fill = Brushes.Black;
                                        shape.SetBinding(UIElement.VisibilityProperty, new Binding("IsFocused") { Source = this.GraphicalShape, Converter = BoolVisibilityConverter });
                                        shape.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
                                        shape.MouseMove += Shape_MouseMove;
                                        shape.MouseLeftButtonUp += Shape_MouseLeftButtonUp;
                                        OwnerChart.ViewPanel.Children.Add(shape);
                                    }
                                }
                                break;
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                                foreach (var item in e.OldItems)
                                {
                                    if (item is Shape shape)
                                    {
                                        shape.MouseLeftButtonDown -= Shape_MouseLeftButtonDown;
                                        shape.MouseMove -= Shape_MouseMove;
                                        OwnerChart.ViewPanel.Children.Remove(shape);
                                    }
                                }
                                break;
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                                foreach (var item in e.OldItems)
                                {
                                    if (item is Shape shape)
                                    {
                                        shape.MouseLeftButtonDown -= Shape_MouseLeftButtonDown;
                                        shape.MouseMove -= Shape_MouseMove;
                                        OwnerChart.ViewPanel.Children.Remove(shape);
                                    }
                                }
                                foreach (var item in e.NewItems)
                                {
                                    if (item is Shape shape)
                                    {
                                        shape.Fill = Brushes.Black;
                                        shape.SetBinding(UIElement.VisibilityProperty, new Binding("IsFocused") { Source = this.GraphicalShape, Converter = BoolVisibilityConverter });
                                        shape.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
                                        shape.MouseMove += Shape_MouseMove;
                                        OwnerChart.ViewPanel.Children.Add(shape);
                                    }
                                }
                                break;
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                                break;
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                                break;
                            default:
                                break;
                        }
                    };
                }
                return controlShapes;
            }
        }


        /// <summary>
        /// 门控制点按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GraphicalMoveStartPoint = default;
            e.Handled = true;
        }
        /// <summary>
        /// 门控制点移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// 门控制按钮鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RefreshSubset();
        }
        /// <summary>
        /// 绘制控制按钮
        /// </summary>
        protected abstract void DrawingControl();
        /// <summary>
        /// 监控数据模型,数据变化时，可能需要重新绘制门的控制按钮
        /// </summary>
        /// <param name="model"></param>
        protected abstract void MonitorControlData(BaseGraphicalModel model);
        #endregion

        #region 门内数据集
        private IEnumerable<IList> subset;
        /// <summary>
        /// 门内数据集
        /// </summary>
        public IEnumerable<IList> Subset
        {
            get { return subset; }
            set { subset = value; OnPropertyChanged("Subset"); }
        }

        /// <summary>
        /// 刷新门内数据
        /// </summary>
        public abstract void RefreshSubset();
        #endregion
    }

    public abstract class BaseGraphicalModel : NotifyPropertyChanged
    {
        #region property
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        #endregion
        public BaseGraphicalModel() { }

    }
}
