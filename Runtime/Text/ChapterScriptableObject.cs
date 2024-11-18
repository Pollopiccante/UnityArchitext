using UnityEngine;

[CreateAssetMenu(fileName = "Chapter", menuName = "ScriptableObjects/ChapterScriptableObject", order = 1)]
public class ChapterScriptableObject : TextScriptableObject
{
    public string chapterTitle;
    public string content;

    public override string GetContent()
    {
        return content;
    }
}
