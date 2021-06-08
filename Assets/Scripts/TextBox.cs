using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBox : MonoBehaviour
{
    [SerializeField] public int framesPerChar;
    [SerializeField] public bool freezePlayer; // When true, the player cannot move while the textbox is on screen. Consider this the default

    //public string[] texts;
    public List<string> texts;
    public int slideIndex;
    //private int highestTextIndex = 0;
    public Text textReference;

    // animation vars
    public bool maskText; // When true, messages are revealed one letter at a time
    private bool textStillRevealing = true; // text is still in the process of being revealed
    public int characterIndex;
    private int mini_counter;
    private readonly int MAX_TEXT_BOX_COUNT = 15;

    // Start is called before the first frame update
    void Start()
    {
        texts = new List<string>() { "Cum!"};
        //texts = new string[MAX_TEXT_BOX_COUNT];

        slideIndex = 0;
        characterIndex = 0;

        /*
        textReference = this.gameObject.GetComponent<Text>();
        if (textReference != null)
        {
            textReference.text = texts[0];
        }
        else
        {
            Debug.LogError("No Text component found on TextBox object!");
        }
        */
    }

    /*
    private void Awake()
    {
        //texts = new string[MAX_TEXT_BOX_COUNT];

        slideIndex = 0;
        characterIndex = 0;
    }
    */

    // Update is called once per frame
    void FixedUpdate()
    {
        if (slideIndex < 0 || slideIndex > texts.Count)
            slideIndex = 0;

        if (texts.Count == 0)
        {
            Debug.Log("TextBox Texts list has no contents");
            //texts.Add("I'm so fucking pissed rn");
            texts = GameManager.Instance.player.currentTextBox.texts;
            slideIndex = 0;
            //return;
        }
        /*
        if (highestTextIndex == 0) 
        {
            Debug.Log("TextBox Texts array has a highest index of zero.");
            return;
        }
        */

        if (mini_counter % framesPerChar == 0)
            characterIndex++;

        Debug.Log("slideIndex:"+slideIndex);

        if (characterIndex > texts[slideIndex].Length)
            characterIndex = texts[slideIndex].Length;

        if (maskText && textStillRevealing)
        {
            if (textReference == null)
                Debug.Log("Textref null");
            else if (texts == null)
                Debug.Log("List err");

            textReference.text = texts[slideIndex].Substring(0, characterIndex);
            if (characterIndex == texts[slideIndex].Length)
                textStillRevealing = false;
        }
        else if (!maskText)
        {
            textReference.text = texts[slideIndex];
        }
    }

    public void SetTexts(string[] text)
    {
        /*
        highestTextIndex = text.Length - 1;
        for(int i = 0; i < text.Length; i++)
        {
            this.texts[i] = string.Copy(text[i]);
        }
        */

        //this.texts = new List<string>();
        for(int i = 0; i < text.Length; i++)
        {
            this.texts.Add(string.Copy(text[i]));
        }
    }

    // DOES NOTHING DON'T USE
    public void AddText(string text)
    {
        //this.texts.Add(text);
    }

    /// <summary>
    /// Advances the text box to the next slide.
    /// </summary>
    /// <returns>True when the dialog has finished.</returns>
    public bool NextSlide()
    {
        //if (highestTextIndex == 0) return true;
        
        if (textStillRevealing)
        {
            characterIndex = texts[slideIndex].Length;
            Debug.Log("Text was still revealing");
        }
        else
        {
            Debug.Log("Next slide");
            if (slideIndex == texts.Count - 1)
                return true;
            else
            {
                slideIndex++;
                characterIndex = 0;
                textStillRevealing = true;
            }
        }
        return false;
    }

    public void ResetBox()
    {
        slideIndex = characterIndex = 0;
        //texts = new List<string>();
        texts.Clear();
        //texts = new string[MAX_TEXT_BOX_COUNT];
        //highestTextIndex = 0;
        maskText = true;
        textStillRevealing = true;
        mini_counter = 0;
    }
}
