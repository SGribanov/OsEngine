using System.Windows.Automation;

namespace OsEngine.TesterAutomation.Runtime;

internal sealed class UiAutomationDriver
{
    public AutomationElement WaitForWindow(UiTarget target, TimeSpan timeout)
    {
        return WaitForElementCore(() => FindWindow(target), timeout, $"Window '{target}' was not found.");
    }

    public bool IsWindowPresent(UiTarget target)
    {
        return FindWindow(target) is not null;
    }

    public void WaitForElementEnabled(UiTarget target, TimeSpan timeout)
    {
        WaitForElementCore(
            () =>
            {
                AutomationElement? element = FindElement(target);
                return element is not null && element.Current.IsEnabled ? element : null;
            },
            timeout,
            $"Element '{target}' did not become enabled.");
    }

    public void Click(UiTarget target, TimeSpan timeout)
    {
        AutomationElement element = WaitForElement(target, timeout);

        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokePattern))
        {
            ((InvokePattern)invokePattern).Invoke();
            return;
        }

        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionPattern))
        {
            ((SelectionItemPattern)selectionPattern).Select();
            return;
        }

        throw new InvalidOperationException($"Element '{target}' does not support click/invoke patterns.");
    }

    public void SetValue(UiTarget target, string value, TimeSpan timeout)
    {
        AutomationElement element = WaitForElement(target, timeout);

        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
        {
            ((ValuePattern)valuePattern).SetValue(value);
            return;
        }

        throw new InvalidOperationException($"Element '{target}' does not support ValuePattern.");
    }

    public void SelectComboBoxItem(UiTarget target, string value, TimeSpan timeout)
    {
        AutomationElement comboBox = WaitForElement(target, timeout);

        if (comboBox.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandCollapsePattern))
        {
            ExpandCollapsePattern expandCollapse = (ExpandCollapsePattern)expandCollapsePattern;
            if (expandCollapse.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
            {
                expandCollapse.Expand();
            }
        }

        Condition itemCondition = new PropertyCondition(AutomationElement.NameProperty, value);
        AutomationElement item = WaitForElementCore(
            () => comboBox.FindFirst(TreeScope.Descendants, itemCondition),
            timeout,
            $"Combo box item '{value}' was not found for '{target}'.");

        if (item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionPattern))
        {
            ((SelectionItemPattern)selectionPattern).Select();
            return;
        }

        if (item.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokePattern))
        {
            ((InvokePattern)invokePattern).Invoke();
            return;
        }

        throw new InvalidOperationException($"Combo box item '{value}' does not support select/invoke patterns.");
    }

    public void CloseWindow(UiTarget target, TimeSpan timeout)
    {
        AutomationElement window = WaitForWindow(target, timeout);

        if (window.TryGetCurrentPattern(WindowPattern.Pattern, out object? windowPattern))
        {
            ((WindowPattern)windowPattern).Close();
            return;
        }

        throw new InvalidOperationException($"Window '{target}' does not support WindowPattern.");
    }

    private AutomationElement WaitForElement(UiTarget target, TimeSpan timeout)
    {
        return WaitForElementCore(() => FindElement(target), timeout, $"Element '{target}' was not found.");
    }

    private AutomationElement? FindElement(UiTarget target)
    {
        AutomationElement root = FindWindow(target) ?? AutomationElement.RootElement;

        if (string.IsNullOrWhiteSpace(target.AutomationId))
        {
            return root;
        }

        Condition condition = new PropertyCondition(AutomationElement.AutomationIdProperty, target.AutomationId);
        return root.FindFirst(TreeScope.Descendants, condition);
    }

    private AutomationElement? FindWindow(UiTarget target)
    {
        if (string.IsNullOrWhiteSpace(target.WindowAutomationId) == false)
        {
            Condition byAutomationId = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.AutomationIdProperty, target.WindowAutomationId));

            AutomationElement? window = AutomationElement.RootElement.FindFirst(TreeScope.Children, byAutomationId);
            if (window is not null)
            {
                return window;
            }
        }

        if (string.IsNullOrWhiteSpace(target.WindowName) == false)
        {
            Condition byName = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.NameProperty, target.WindowName));

            return AutomationElement.RootElement.FindFirst(TreeScope.Children, byName);
        }

        return null;
    }

    private static AutomationElement WaitForElementCore(
        Func<AutomationElement?> probe,
        TimeSpan timeout,
        string failureMessage)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow <= deadline)
        {
            try
            {
                AutomationElement? element = probe();
                if (element is not null)
                {
                    return element;
                }
            }
            catch (ElementNotAvailableException)
            {
                // Ignore and retry.
            }

            Thread.Sleep(250);
        }

        throw new TimeoutException(failureMessage);
    }
}

internal sealed class UiTarget
{
    public string? WindowAutomationId { get; init; }

    public string? WindowName { get; init; }

    public string? AutomationId { get; init; }

    public override string ToString()
    {
        return $"{WindowAutomationId ?? WindowName ?? "<root>"}::{AutomationId ?? "<window>"}";
    }
}
