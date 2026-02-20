namespace IFoxCAD.Cad;

/// <summary>
/// 属性文字扩展
/// </summary>
public static class AttributeDefinitionEx
{
    /// <summary>
    /// 设置属性为多行文字，并通过委托设置多行文字的属性
    /// </summary>
    /// <param name="attr">属性对象</param>
    /// <param name="action">设置多行文字的委托</param>
    public static void SetMTextAttribute(this AttributeDefinition attr, Action<MText> action)
    {
        attr.IsMTextAttributeDefinition = true;
        var mt = attr.MTextAttributeDefinition;
        action(mt);
        attr.MTextAttributeDefinition = mt;
        attr.UpdateMTextAttributeDefinition();
    }
        
}