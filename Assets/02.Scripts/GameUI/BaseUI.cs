using System;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public abstract UIType UIType { get; }

    protected virtual void Awake()
    {
        UIManager.Instance.RegisterUI(this);
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    protected virtual void Refresh() { }

}
