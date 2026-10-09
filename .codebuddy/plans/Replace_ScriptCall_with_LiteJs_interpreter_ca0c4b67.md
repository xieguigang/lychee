---
name: Replace ScriptCall with LiteJs interpreter
overview: 删除 LycheeUI 的 ScriptCall.vb 手写 JS 解析模块，改用已引用的 LiteJs.vbproj 解释器引擎来解析与执行 HTML 事件属性中的 JavaScript，并通过反射把宿主控件（container 及其各级父控件 / AddTarget 对象）上的方法注册为解释器的全局函数，使 JS 可直接调用宿主方法。
todos:
  - id: delete-scriptcall
    content: 删除 ScriptCall.vb 并从工程移除该文件
    status: completed
  - id: add-define-global
    content: 在 LiteJs 的 Interpreter 新增 DefineGlobal 方法注入宿主委托
    status: completed
  - id: rewrite-methodbinder
    content: 重写 MethodBinder 用 LiteJs 解析执行并反射绑定宿主方法
    status: completed
    dependencies:
      - delete-scriptcall
      - add-define-global
  - id: build-verify
    content: 编译 LycheeUI 工程确认通过并验证事件调用
    status: completed
    dependencies:
      - rewrite-methodbinder
---

## 用户需求

删除基于手工正则的 `src\LycheeUI\Events\ScriptCall.vb` JavaScript 解析模块，改用用户自研的、已引用的 LiteJs 解释器引擎（`G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\JavaScript\LiteJs.vbproj`）来解析并执行 HTML 事件属性中的完整 JavaScript 代码，并将宿主控件上的方法通过反射绑定到解释器中供 JS 直接调用。

## 核心功能

- 删除 `ScriptCall.vb`，不再使用 `methodName(args)` 形式的极简手写解析。
- `MethodBinder` 改由 LiteJs 的 `Parser` + `Interpreter` 执行任意完整 JS 表达式/语句。
- 通过反射将 container 及其各级父控件、`AddTarget` 追加对象上的实例方法注册为解释器全局函数，使 JS 可直接调用宿主方法并正确完成实参类型转换。
- 保持现有对外契约：`Sub New(container As Control)`、`AddTarget(target)`、`Invoke(expression As String) As Boolean`、`LastError` 属性，确保 `FormRender.vb` 无需改动即可继续工作。

## 技术栈

- 宿主项目：`g:\lychee\src\LycheeUI\LycheeUI.vbproj`（net10.0-windows，WinForms，UseWindowsForms=true），已通过第 26 行 `<ProjectReference>` 引用 LiteJs 项目，无需新增工程引用。
- 解释器引擎：`Microsoft.VisualBasic.ApplicationServices.VM.JavaScript`（LiteJs，纯 VB.NET，net10.0，仅 BCL）。
- 关键 API：`Parser.Parse(source) As Program`、`Interpreter.New(Optional io As ScriptIO)`、`Interpreter.Run(program As Program)`。
- 可调用值约束：`JsRuntime.JsInvoke` 仅接受 `Func(Of Object(), Object)` 或 `BuiltinMethod`，故宿主方法必须包装为 `Func(Of Object(), Object)` 委托。

## 实现方案

### 总体策略

用「解释器 + 反射注入全局函数」替代「手写解析 + 反射直接调用」。在 `MethodBinder` 构造时创建 `Interpreter` 实例，遍历宿主查找列表（container 及其各级 Parent、以及 `AddTarget` 追加对象），通过反射枚举其实例方法，将每个方法名注册为解释器的一个全局 `Func(Of Object(), Object)` 闭包。事件触发时，`Invoke(expression)` 把 HTML 事件属性文本（剥离 `javascript:` 前缀、空串短路）交给 `Parser.Parse` 生成 AST，再 `Interpreter.Run` 执行；JS 中对宿主方法的调用会落到对应闭包，由闭包完成 JS 值到 .NET 参数的双向转换并 `method.Invoke`。

### 关键技术决策

1. **扩展 Interpreter 暴露注入入口**：现有 `_globals As Environment` 为 Private 且无 Public 修改器，无法注入宿主委托。在 `Interpreter` 上新增最小 `Public Sub DefineGlobal(name As String, value As Object)`，内部调用 `_globals.Define(name, value, False)`。此改动位于用户自有引擎，最少侵入，且语义清晰（与解释器内置全局标识符走同一路径）。
2. **方法名冲突处理**：旧 `MethodBinder.Resolve` 按「名字 + 参数个数」在 targets 中依次查找、首个命中（最近祖先优先）。重写时对每个 target 调用 `RegisterHost`，并按「container（及其 Parent）先于 AddTarget」的顺序注册；同一名字一旦注册即跳过后续，保证最近祖先方法胜出，行为与旧逻辑一致。
3. **重载解析**：旧逻辑仅按参数个数匹配单方法。宿主方法可能存在重载，闭包在调用时按 JS 实参个数/可赋值性挑选最匹配 `MethodInfo`，提升鲁棒性（仍保留旧按个数优先的简单策略作为兜底）。
4. **参数双向转换**：

- JS→CLR（`ConvertJsToClr`）：`Double`→目标数值/Integer（截断）、`String`→String、`Boolean`→Boolean、`List(Of Object)`/`Dictionary`→Object（或集合类型）、`Nothing`/`Undef`→Nothing（值类型用默认值）。转换失败回退为原样传入，由 `method.Invoke` 抛错并写入 `LastError`。
- CLR→JS（`ConvertClrToJs`）：基本类型/字符串/集合与 JS 值类型互转，无返回值的方法返回 `JsRuntime.Undef`。

5. **异常与 LastError**：`Invoke` 中解析/运行异常统一捕获，写入 `LastError`（优先 `InnerException.Message`，回退 `Message`），返回 `False`，保持 `FormRender` 现有的 `[lychee] {binder.LastError}` 日志与 `OnClick` 触发逻辑不变。

### 性能与可靠性

- 反射枚举与方法选择缓存：`MethodBinder` 构造时一次性枚举并注册全部宿主方法，运行期仅 JSON 解析 + 树遍历（AST 已在 LiteJs 内缓存无关的纯解析），事件高频点击无反射开销。
- 与旧实现对比：旧实现每次 `Invoke` 都做 `Resolve`（带缓存）与 `Convert.ChangeType`；新实现把反射成本前移到构造，运行期更平滑。
- 兼容性：保留 `LastError`、`AddTarget`、`Sub New(container As Control)`，移除仅在内部使用的 `Invoke(script As ScriptCall)` 重载（ScriptCall 已删除），对外行为不变。

## 实施注意事项

- 确认 `ScriptCall` 在全项目仅被 `MethodBinder` 引用（已搜索验证），删除后无悬空引用。
- 注册宿主方法时应跳过 `Equals`/`GetHashCode`/`GetType`/`ToString` 等 `Object` 基础方法，避免污染/冲突（可选，建议跳过 `Object` 基类声明的方法）。
- 解释器全局环境区分大小写，JS 调用必须与宿主方法名大小写一致（旧 `Resolve` 忽略大小写；如需保留忽略大小写，可在 `RegisterHost` 时用 `ToLower` 规范化名字，但需统一 JS 侧约定，建议保持原样大小写以贴合 JS 语义）。
- 不要改动 `LycheeUI.vbproj` 的现有 ProjectReference（已包含 LiteJs）。

## 目录结构与改动文件

```
g:\lychee\src\LycheeUI\
├── Events/
│   ├── ScriptCall.vb     # [删除] 手写极简 JS 解析模块，被 ScriptCall 替代后不再需要
│   └── MethodBinder.vb   # [重写] 改用 LiteJs 解释器 + 反射注入宿主方法为全局函数，
│                         #        保留 Sub New/AddTarget/Invoke(String)/LastError 对外契约

G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\JavaScript\
└── Interpreter.vb        # [修改] Interpreter 类新增 Public Sub DefineGlobal(name, value)，
                         #        委托 _globals.Define 注入宿主委托/值
```

（`FormRender.vb` 等调用方无需改动，依赖的是 `MethodBinder` 既有公开契约。）

## 关键代码结构（新增的公共 API）

```
' Interpreter.vb（LiteJs 引擎）
Public NotInheritable Class Interpreter
    ''' <summary>将宿主侧值（如 Func(Of Object(), Object) 委托）注册为脚本可见的全局标识符。</summary>
    Public Sub DefineGlobal(name As String, value As Object)
End Sub
End Class

' MethodBinder.vb（LycheeUI）
Public NotInheritable Class MethodBinder
    Public Sub New(container As Control)
    Public Sub AddTarget(target As Object)
    Public Function Invoke(expression As String) As Boolean
    Public Property LastError As String
End Class
```