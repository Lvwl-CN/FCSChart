# FCSChart
 FCS文件图表控件
 
 图表类型有： 散点图（ScatterChart）、直方图（HistogramChart）、密度图（DensityChart）、等高线图（ContourChart）
 
 轴类型有：线性（Line）、对数（Log10）、负数对数（Log10Biex）
 
 门类型有：多边形门（PolygonGraphical）、四边形门（QuadrilateralGraphical）、段选门（VerticalGraphical）
 
![效果图](https://github.com/Lvwl-CN/FCSChart/blob/master/doc/charts.png)

## 图表主要属性 ##
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | ItemsSource | 数据源 | IEnumerable<IList> | 依赖属性，IList里的数据必须为可转成double的数据 |
 | Parameters | 参数集合 | IEnumerable | 依赖属性，FCS的参数集合，可对其在XY轴的选项控件（ComboBox）指定数据模板（XParamDataTemplate、YParamDataTemplate） |
 | PolygonCommand | 画多边形门命令 | ICommand |  |
 | QuadrilateralCommand | 画四边形门命令 | ICommand |  |
 | VerticalCommand | 画段选门命令 | ICommand |  |
