using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CodeWF;

class UIService : IUIService
{
    public Type GetInputType(Type dataType, FieldType fieldType)
    {
        throw new NotImplementedException();
    }

    public void AddInputAttributes<TItem>(Dictionary<string, object> attributes, FieldModel<TItem> model) where TItem : class, new()
    {
        throw new NotImplementedException();
    }

    public Task Toast(string message, StyleType style = StyleType.Success)
    {
        throw new NotImplementedException();
    }

    public Task Notice(string message, StyleType style = StyleType.Success)
    {
        throw new NotImplementedException();
    }

    public bool Alert(string message, Func<Task> action = null)
    {
        throw new NotImplementedException();
    }

    public bool Confirm(string message, Func<Task> action)
    {
        throw new NotImplementedException();
    }

    public bool ShowDialog(DialogModel model)
    {
        throw new NotImplementedException();
    }

    public bool ShowForm<TItem>(FormModel<TItem> model) where TItem : class, new()
    {
        throw new NotImplementedException();
    }

    public void BuildForm<TItem>(RenderTreeBuilder builder, FormModel<TItem> model) where TItem : class, new()
    {
        throw new NotImplementedException();
    }
    public void BuildSpin(RenderTreeBuilder builder, SpinModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildToolbar(RenderTreeBuilder builder, ToolbarModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildQuery(RenderTreeBuilder builder, TableModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildTable<TItem>(RenderTreeBuilder builder, TableModel<TItem> model) where TItem : class, new()
    {
        throw new NotImplementedException();
    }

    public void BuildTree(RenderTreeBuilder builder, TreeModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildSteps(RenderTreeBuilder builder, StepModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildTabs(RenderTreeBuilder builder, TabModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildDropdown(RenderTreeBuilder builder, DropdownModel model)
    {
        throw new NotImplementedException();
    }

    public void BuildAlert(RenderTreeBuilder builder, string text, StyleType type = StyleType.Info)
    {
        throw new NotImplementedException();
    }

    public void BuildTag(RenderTreeBuilder builder, string text, string color = null)
    {
        throw new NotImplementedException();
    }

    public void BuildIcon(RenderTreeBuilder builder, string type, EventCallback<MouseEventArgs>? onClick = null)
    {
        throw new NotImplementedException();
    }

    public void BuildResult(RenderTreeBuilder builder, string status, string message)
    {
        throw new NotImplementedException();
    }

    public void BuildButton(RenderTreeBuilder builder, ActionInfo info)
    {
        throw new NotImplementedException();
    }

    public void BuildSearch(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildText(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildTextArea(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildPassword(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildDatePicker<TValue>(RenderTreeBuilder builder, InputModel<TValue> model)
    {
        throw new NotImplementedException();
    }

    public void BuildNumber<TValue>(RenderTreeBuilder builder, InputModel<TValue> model)
    {
        throw new NotImplementedException();
    }

    public void BuildCheckBox(RenderTreeBuilder builder, InputModel<bool> model)
    {
        throw new NotImplementedException();
    }

    public void BuildSwitch(RenderTreeBuilder builder, InputModel<bool> model)
    {
        throw new NotImplementedException();
    }

    public void BuildSelect(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildRadioList(RenderTreeBuilder builder, InputModel<string> model)
    {
        throw new NotImplementedException();
    }

    public void BuildCheckList(RenderTreeBuilder builder, InputModel<string[]> model)
    {
        throw new NotImplementedException();
    }

    public Language Language { get; set; }
}