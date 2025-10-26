# WpfImageClipper

一个用于 WPF 应用程序的交互式图像裁剪控件，支持使用贝塞尔曲线绘制任意形状的裁剪区域。

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![WPF](https://img.shields.io/badge/WPF-Windows-blue.svg)

## ✨ 特性

- 🎨 **自由绘制裁剪路径** - 使用贝塞尔曲线绘制任意形状
- 🔧 **三种贝塞尔曲线支持**
  - 线性贝塞尔（直线）
  - 二次贝塞尔曲线（单控制点）
  - 三次贝塞尔曲线（双控制点）
- 🎯 **精确的曲线点击检测** - 使用牛顿迭代法实现精确的 HitTest
- ✂️ **无损曲线插入** - 使用 De Casteljau 算法保持曲线形状不变
- 🖱️ **直观的交互方式** - 键盘修饰键配合鼠标操作
- 📐 **实时预览** - 所见即所得的裁剪效果

## 🎬 快速开始

### 运行示例项目

1. 克隆仓库
```bash
git clone https://github.com/SlimeNull/WpfImageClipper.git
cd WpfImageClipper
```

2. 打开解决方案
```bash
# 使用 Visual Studio 2022 或更高版本
start WpfImageClipper.sln
```

3. 构建并运行 `WpfImageClipper` 项目

### 基本使用方法

#### 1️⃣ 加载图像
- 点击 **"打开文件"** 按钮选择要裁剪的图像

#### 2️⃣ 绘制裁剪路径
- **添加点** - 直接点击鼠标左键
- **在曲线上插入点** - 点击现有曲线段
- **删除点** - 点击要删除的点（第一个点除外）
- **闭合路径** - 点击第一个点

#### 3️⃣ 调整贝塞尔控制点
- **创建贝塞尔点** - `Alt + 拖动` 刚添加的点
- **调整控制点** - `Alt + 拖动` 贝塞尔点（会同时调整入点和出点）
- **单独移动控制点** - `Ctrl + 点击并拖动` 控制点手柄

#### 4️⃣ 移动点
- **移动点** - `Ctrl + 拖动` 点

#### 5️⃣ 导出裁剪结果
- 点击 **"导出"** 按钮保存裁剪后的图像

## 🔧 在自己的项目中使用

### 方法一：引用项目

1. 将 `LibBezierCurve` 和 `WpfImageClipper` 项目添加到你的解决方案

2. 在你的 WPF 项目中添加项目引用：
```xml
<ItemGroup>
  <ProjectReference Include="..\WpfImageClipper\WpfImageClipper.csproj" />
</ItemGroup>
```

### 方法二：使用控件

#### 在 XAML 中使用 ImageClipper

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 xmlns:controls="clr-namespace:WpfImageClipper.Controls;assembly=WpfImageClipper"
        Title="图像裁剪器" Height="600" Width="800">
    <Grid>
     <controls:ImageClipper x:Name="imageClipper" />
    </Grid>
</Window>
```

#### 在代码中使用

```csharp
using System.Windows;
using System.Windows.Media.Imaging;
using WpfImageClipper.Controls;

namespace YourApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
   InitializeComponent();
        }

        // 加载图像
        private void LoadImage()
        {
    var bitmap = new BitmapImage(new Uri("path/to/image.jpg"));
    imageClipper.Source = bitmap;
        }

        // 获取裁剪后的图像
   private void ExportClippedImage()
        {
BitmapSource? clippedImage = imageClipper.GetClippedImage();
          
        if (clippedImage != null)
       {
     // 保存到文件
                using var stream = System.IO.File.Create("output.png");
        var encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(clippedImage));
                encoder.Save(stream);
            }
   }

        // 清除路径
  private void ClearPath()
      {
          imageClipper.AreaPoints.Clear();
  imageClipper.IsAreaClosed = false;
        }

    // 以编程方式添加点
private void AddPoint(double x, double y)
        {
            imageClipper.AreaPoints.Add(new ImageClipAreaCommonPoint
   {
  Position = new Point(x, y)
            });
        }

        // 添加贝塞尔点
 private void AddBezierPoint(double x, double y, double cp1X, double cp1Y, double cp2X, double cp2Y)
    {
            imageClipper.AreaPoints.Add(new ImageClipAreaBezierPoint2
      {
         Position = new Point(x, y),
     ControlPoint1 = new Point(cp1X, cp1Y),
       ControlPoint2 = new Point(cp2X, cp2Y)
  });
        }
  }
}
```

### 可用的点类型

```csharp
using WpfImageClipper.Controls;

// 1. 普通点（直线连接）
var commonPoint = new ImageClipAreaCommonPoint
{
    Position = new Point(100, 100)
};

// 2. 贝塞尔点（只有出点）
var bezierPoint1 = new ImageClipAreaBezierPoint1
{
 Position = new Point(200, 200),
    ControlPoint2 = new Point(220, 180)  // 出点控制点
};

// 3. 贝塞尔点（入点和出点）
var bezierPoint2 = new ImageClipAreaBezierPoint2
{
    Position = new Point(300, 300),
  ControlPoint1 = new Point(280, 320),  // 入点控制点
    ControlPoint2 = new Point(320, 280)   // 出点控制点
};
```

## 🎮 操作指南

| 操作 | 说明 |
|------|------|
| **鼠标左键点击** | 添加新点 |
| **点击曲线段** | 在曲线上插入新点 |
| **点击点** | 删除点（第一个点除外）<br>点击第一个点闭合路径 |
| **Alt + 拖动新点** | 创建贝塞尔控制点 |
| **Alt + 拖动贝塞尔点** | 同时调整入点和出点 |
| **Ctrl + 拖动点** | 移动点位置 |
| **Ctrl + 拖动控制点** | 单独移动控制点 |

## 🏗️ 项目结构

```
WpfImageClipper/
├── LibBezierCurve/   # 贝塞尔曲线数学库
│   ├── CubicBezierCurve.cs   # 三次贝塞尔曲线
│   ├── QuadraticBezierCurve.cs # 二次贝塞尔曲线
│   ├── LinearBezierCurve.cs    # 线性贝塞尔（直线）
│   ├── IBezierCurve.cs   # 贝塞尔曲线接口
│   └── Internal/
│       └── MathUtils.cs         # 数学工具类
├── WpfImageClipper/   # WPF 应用程序
│   ├── Controls/
│   │   ├── ImageClipper.cs     # 主控件
│   │   ├── ImageClipAreaPoint.cs
│   │   ├── ImageClipAreaCommonPoint.cs
│   │   ├── ImageClipAreaBezierPoint1.cs
│   │   ├── ImageClipAreaBezierPoint2.cs
│   │   └── ImageClipAreaPointCollection.cs
│   └── MainWindow.xaml          # 主窗口
└── README.md      # 本文档
```

## 🔬 技术亮点

### 1. 精确的曲线点击检测

使用**牛顿迭代法**求解点到曲线的最短距离：

```csharp
// 目标：最小化 D(t) = (Bx(t) - x)² + (By(t) - y)²
// 使用牛顿法求解 D'(t) = 0
public bool HitTest(double x, double y, double threshold, out double t)
{
    // 多起点采样 + 牛顿迭代
    // 保证找到全局最优解
}
```

### 2. 无损曲线细分

使用 **De Casteljau 算法**在插入点时保持曲线形状：

```csharp
// 将曲线在参数 t 处细分为两段
// 数学上保证细分后的曲线与原曲线完全一致
public void Subdivide(double t, 
    out CubicBezierCurve leftCurve, 
    out CubicBezierCurve rightCurve)
{
    // De Casteljau 递归插值
}
```

### 3. 高阶方程求解

支持求解一元三次方程（用于贝塞尔曲线参数计算）：

```csharp
// 使用 Cardano 公式求解三次方程
// 处理所有情况：1个实根、3个实根、重根
public static IEnumerable<double> ResolveEquation(
    double a, double b, double c, double d)
```

## 📋 系统要求

- **.NET 8.0** 或更高版本
- **Windows** 操作系统（WPF 限制）
- **Visual Studio 2022** 或更高版本（推荐）

## 🎓 核心概念

### 贝塞尔曲线类型

1. **线性贝塞尔（直线）**
   - 两个端点
   - 最简单的形式

2. **二次贝塞尔**
   - 两个端点 + 一个控制点
   - 可以形成简单的曲线

3. **三次贝塞尔**
   - 两个端点 + 两个控制点
   - 最灵活，可以形成 S 形曲线

### 点的类型层次

```
ImageClipAreaPoint (抽象基类)
├── ImageClipAreaCommonPoint (普通点)
└── ImageClipAreaBezierPoint1 (带出点)
    └── ImageClipAreaBezierPoint2 (带入点和出点)
```

## 🤝 贡献

欢迎提交 Issues 和 Pull Requests！

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 👤 作者

**SlimeNull**
- GitHub: [@SlimeNull](https://github.com/SlimeNull)
- 项目地址: https://github.com/SlimeNull/WpfImageClipper

## 🙏 致谢

- 贝塞尔曲线数学理论
- De Casteljau 算法
- 牛顿迭代法

## 📚 相关资源

- [贝塞尔曲线 - 维基百科](https://zh.wikipedia.org/wiki/%E8%B2%9D%E8%8C%B2%E6%9B%B2%E7%B7%9A)
- [De Casteljau 算法](https://en.wikipedia.org/wiki/De_Casteljau%27s_algorithm)
- [WPF 官方文档](https://docs.microsoft.com/zh-cn/dotnet/desktop/wpf/)

---

⭐ 如果这个项目对你有帮助，请给一个 Star！
