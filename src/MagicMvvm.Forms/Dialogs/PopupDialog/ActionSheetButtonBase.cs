﻿namespace MagicMvvm.Dialogs;

/// <summary>
/// ActionSheetButton Base class
/// </summary>
public abstract class ActionSheetButtonBase : IActionSheetButton
{
    protected ActionSheetButtonBase()
    {
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="IActionSheetButton"/>
    /// is cancel.
    /// </summary>
    /// <value><c>true</c> if is cancel; otherwise, <c>false</c>.</value>
    protected bool _isCancel { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="IActionSheetButton"/>
    /// is destroy.
    /// </summary>
    /// <value><c>true</c> if is destroy; otherwise, <c>false</c>.</value>
    protected bool _isDestroy { get; private set; }

    /// <summary>
    /// The backing text for <see cref="IActionSheetButton"/>
    /// </summary>
    /// <value>The text.</value>
    protected string _text { get; private set; }

    /// <summary>
    /// The button will be used as a Cancel Button
    /// </summary>
    /// <value><c>true</c> if is cancel; otherwise, <c>false</c>.</value>
    public bool IsCancel
    {
        get => _isCancel;
        protected internal set
        {
            if (_isCancel == value)
                IsDestroy = false;
        }
    }

    /// <summary>
    /// The button will be used as a Destroy Button
    /// </summary>
    /// <value><c>true</c> if is destroy; otherwise, <c>false</c>.</value>
    public bool IsDestroy
    {
        get => _isDestroy;
        protected internal set
        {
            if (_isDestroy == value)
                IsCancel = false;
        }
    }

    /// <summary>
    /// Executes the action to take when the button is pressed
    /// </summary>
    /// <value>The text.</value>
    public string Text
    {
        get => _text;
        protected internal set => _text = value;
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    protected abstract void OnButtonPressed();
        
    bool IActionSheetButton.IsCancel => _isCancel;
    bool IActionSheetButton.IsDestroy => _isDestroy;
    string IActionSheetButton.Text => _text;
    void IActionSheetButton.PressButton() => OnButtonPressed();
}