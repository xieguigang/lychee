---
name: Add GetElementById to FormRender
overview: 在 LycheeUI 的 FormRender 类中新增一个与 DOM 语义一致的 GetElementById(id) As UiBox 方法，按元素的 id 属性值查找并返回对应的 UiBox 节点对象（复用现有 layout.FindById），从而使宿主代码可以通过 form.GetElementById("name").Value 直接取得输入框中用户输入的字符串。保留已有的 FindById 以维持向后兼容。
todos:
  - id: add-getelementbyid
    content: 在 FormRender.vb 的 FindById 附近新增 GetElementById(id As String) As UiBox 方法，复用 layout.FindById
    status: completed
  - id: build-verify
    content: 编译 LycheeUI 工程确认 GetElementById 改动通过
    status: completed
    dependencies:
      - add-getelementbyid
  - id: usage-verify
    content: 在 Form1/冒烟测试中用 GetElementById("name").Value 读取输入框值并验证更新
    status: completed
    dependencies:
      - build-verify
---

## 用户需求

在 LycheeUI 项目的 `src\LycheeUI\FormRender.vb` 的 `FormRender` 类中新增一个名为 `GetElementById` 的公共方法，使其可以按照 HTML 元素的 `id` 属性值查找到对应的节点对象（即 `UiBox` 实例）。

## 应用场景

- `test\Form1.vb` 第 75-77 行声明了 `<input type="text" id="name" ... value="lychee" placeholder="user name"/>`（同结构还有 pwd / optA / cb1 等输入控件）。
- 第 126 行 `Dim WithEvents form As New FormRender(UI, Me)` 创建了渲染引擎实例。
- 期望宿主代码能够通过 `form.GetElementById("name")` 拿到该输入框的对象实例，再从中读取用户实际输入的 `value` 字符串（例如 `form.GetElementById("name").Value`）。

## 核心功能

- 按 `id` 查找并返回对应的 `UiBox` 元素对象（而非直接返回值字符串），从而允许调用方进一步访问该元素的 `Value`、属性、类型等任意信息。
- 与现有 `FindById(id)` 行为一致（复用同一查找逻辑），仅使用更符合 DOM 语义的命名。

## 技术现状

- `FormRender` 已存在 `Public Function FindById(id As String) As UiBox`（约 846 行），其实现为 `Return layout.FindById(id)`，即按 `id` 在布局引擎中查找控件并返回 `UiBox`。
- 布局引擎 `UiLayoutEngine.FindById(id)` 已实现完整的按 id 查找，无需改动。
- 元素对象 `UiBox`（`src\LycheeUI\Layout\UiBox.vb`）暴露：
- `Public Property Value As String`（约 59 行）：文本输入框的当前值，即用户想要读取的 "value 字符串"。
- `Attribute(name As String) As String`：读取任意 HTML 属性（如 `id`、`placeholder`）。
- `Public ReadOnly Property InputType / IsTextInput / IsCheckable / Placeholder` 等辅助属性。
- VB 中属性名大小写不敏感，`box.value` 等同 `box.Value`。

## 实现方案

### 总体策略

在 `FormRender` 的 `FindById` 方法附近新增一个名为 `GetElementById` 的 Public 方法，作为 `FindById` 的 DOM 风格别名，内部直接复用 `layout.FindById(id)`，返回 `UiBox`。保持现有 `FindById` 方法不动，以兼容既有调用方与自动化测试。

### 关键决策

1. **复用而非重写**：`GetElementById` 直接 `Return layout.FindById(id)`，避免重复查找逻辑，符合 DRY 与现有架构约定（项目内所有按 id 的操作均基于 `layout.FindById`）。
2. **返回对象而非值**：返回 `UiBox` 而非 `String`，满足用户"先取元素对象再取 value"的诉求；与现有 `GetValue(id)`（返回字符串）形成互补。
3. **命名语义化**：采用 `GetElementById` 与 Web DOM 的 `document.getElementById` 保持一致，降低宿主侧（如 `Form1.vb`）理解成本。
4. **向后兼容**：保留 `FindById`，不破坏任何现存引用。

### 宿主侧用法（实施后）

```
Dim nameBox = form.GetElementById("name")
Dim userValue = nameBox.Value   ' 初始为 "lychee"，用户输入后实时更新
```

## 性能与可靠性

- 查找复杂度与 `UiLayoutEngine.FindById` 一致（内部基于字典/视图索引），为 O(1) 量级，事件高频点击无额外开销。
- 未找到对应 id 时返回 `Nothing`，调用方需自行判空（与 `FindById` 行为一致）；建议在宿主侧加 `If nameBox IsNot Nothing Then` 保护。

## 目录结构与改动文件

```
g:\lychee\src\LycheeUI\
└── FormRender.vb   # [修改] 在 FindById 附近新增 Public Function GetElementById(id As String) As UiBox，
                    #        实现体为 Return layout.FindById(id)；保留 FindById 不变
```

（仅此一个文件改动，不需修改 `UiBox`、`UiLayoutEngine` 或工程引用。）