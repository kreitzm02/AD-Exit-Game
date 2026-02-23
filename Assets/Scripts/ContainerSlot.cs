using UnityEngine;

[System.Serializable]
public class ContainerSlot
{
    public enum ReadableMode
    {
        ManualPages,  
        AutoSplit     
    }

    public UniqueItem_SO item;

    [Header("Readable")]
    public ReadableMode readableMode = ReadableMode.ManualPages;

    public ReadablePage[] readablePages;

    [TextArea(10, 25)]
    public string readableFullText;

    [Min(1)]
    public int readableCharLimitPerPage = 350;

    public bool preserveWords = true;

    public Sprite readableIcon;

    [System.NonSerialized] private ReadablePage[] autoPagesCache;

    public bool HasItem => item != null;

    public bool HasReadable
    {
        get
        {
            var pages = GetReadablePages();
            return pages != null && pages.Length > 0;
        }
    }

    public ReadablePage[] GetReadablePages()
    {
        if (readableMode == ReadableMode.ManualPages)
            return readablePages;

        if (autoPagesCache == null)
            autoPagesCache = BuildAutoPages(readableFullText, readableCharLimitPerPage, preserveWords);

        return autoPagesCache;
    }

    public void InvalidateReadableCache()
    {
        autoPagesCache = null;
    }

    private static ReadablePage[] BuildAutoPages(string text, int limit, bool preserveWords)
    {
        if (string.IsNullOrEmpty(text) || limit <= 0)
            return new ReadablePage[0];

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var list = new System.Collections.Generic.List<ReadablePage>();
        int i = 0;

        while (i < text.Length)
        {
            int remaining = text.Length - i;
            int take = Mathf.Min(limit, remaining);
            int end = i + take;

            if (end < text.Length && preserveWords)
            {
                int lastWhitespace = -1;
                for (int j = end - 1; j > i; j--)
                {
                    if (char.IsWhiteSpace(text[j]))
                    {
                        lastWhitespace = j;
                        break;
                    }
                }

                if (lastWhitespace > i)
                    end = lastWhitespace + 1;
            }

            string pageText = text.Substring(i, end - i).TrimEnd();
            list.Add(new ReadablePage { text = pageText });

            i = end;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;
        }

        return list.ToArray();
    }
}
