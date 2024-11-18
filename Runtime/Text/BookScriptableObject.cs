using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "ScriptableObjects/BookScriptableObject", order = 1)]
public class BookScriptableObject : TextScriptableObject
{
    public string title;
    public string author;
    public ChapterScriptableObject[] chapters;
    public string[] characters;
    
    public BookScriptableObject(string title, string author, ChapterScriptableObject[] chapters, string[] characters)
    {
        this.title = title;
        this.author = author;
        this.chapters = chapters;
        this.characters = characters;
    }

    public void AddChapter(ChapterScriptableObject chapter)
    {
        LinkedList<ChapterScriptableObject> chaptersList = new LinkedList<ChapterScriptableObject>(chapters);
        chaptersList.AddLast(chapter);
        chapters = chaptersList.ToArray();
    }

    public override string GetContent()
    {
        return String.Concat(chapters.Select(chapter => chapter.GetContent()));
    }
}
