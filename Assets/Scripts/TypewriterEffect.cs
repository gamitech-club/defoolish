using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField, Required] private TMP_Text _tmpText;
    [SerializeField] private float _charactersPerSecond = 20;
    [SerializeField] private float _interpunctuationDelay = 0.5f;
    [SerializeField, ReadOnly] private int _currentVisibleCharacterIndex;

    private WaitForSeconds _simpleWaitDelay;
    private WaitForSeconds _interpunctuationWaitDelay;

    public event EventHandler FullTextRevealed;
    public event Action<char> CharacterRevealed;

    private void Reset()
    {
        _tmpText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        Assert.IsNotNull(_tmpText, $"[{name}] TMP_Text not assigned.");

        _simpleWaitDelay = new WaitForSeconds(1 / _charactersPerSecond);
        _interpunctuationWaitDelay = new WaitForSeconds(_interpunctuationDelay);
    }

    // public UniTask WriteAsync(string text, CancellationToken cancellationToken = default)
    // {
    //     _tmpText.maxVisibleCharacters = 0;
    //     _tmpText.text = text;
    //     _currentVisibleCharacterIndex = 0;
    //     return TypewriterCoroutine().WithCancellation(cancellationToken);
    // }

    public void WriteInstantly(string text)
    {
        _tmpText.maxVisibleCharacters = 99999;
        _tmpText.text = text;
        _currentVisibleCharacterIndex = 99999;
    }

    public async UniTask WriteAsync(string text, CancellationToken cancellationToken = default)
    {
        _tmpText.maxVisibleCharacters = 0;
        _tmpText.text = text;
        _currentVisibleCharacterIndex = 0;

        // TextMeshPro stupid, characterCount not updated.
        _tmpText.ForceMeshUpdate();
        var textInfo = _tmpText.textInfo;
        var totalCharCount = textInfo.characterCount;

        while (_currentVisibleCharacterIndex < totalCharCount + 1)
        {
            var lastCharacterIndex = totalCharCount - 1;

            if (_currentVisibleCharacterIndex >= lastCharacterIndex)
            {
                _tmpText.maxVisibleCharacters++;
                FullTextRevealed?.Invoke(this, EventArgs.Empty);
                break;
            }

            char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;
            _tmpText.maxVisibleCharacters++;
            
            if (character == '?' || character == '.' || character == ',' || character == ':' ||
                    character == ';' || character == '!' || character == '-')
            {
                await UniTask.WaitForSeconds(_interpunctuationDelay, cancellationToken: cancellationToken);
            }
            else
            {
                await UniTask.WaitForSeconds(1 / _charactersPerSecond, cancellationToken: cancellationToken);
            }
            
            CharacterRevealed?.Invoke(character);
            _currentVisibleCharacterIndex++;
        }
    }
}
