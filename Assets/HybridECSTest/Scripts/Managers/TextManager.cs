using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance;

    [Header("Turning it on causes more lag")]
    [SerializeField] private bool textMeshOn = false;

    [SerializeField] private GameObject _textContainer;
    private TextMesh _textmesh;
    public float Value { get; private set; }

    Coroutine tempCoroutine;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Value = UnityEngine.Random.value * 100f - 25f;
        _textmesh = _textContainer.GetComponentInChildren<TextMesh>();
        _textmesh.text = Value.ToString("##.#");
    }
  

    public void DisplayScoreFunc(Vector3 pos)
    {
        if(textMeshOn)
        StartCoroutine(DisplayScore(pos));

    }

    private IEnumerator DisplayScore(Vector3 pos)
    {
        GameObject instance = Instantiate(_textContainer, pos, Quaternion.identity);
        if (instance != null)
        {
            var steps = 60;
            instance.transform.localScale = Vector3.zero;
            instance.SetActive(true);
            for (var i = 0; i < steps; i++)
            {
                instance.transform.localScale += Vector3.one / steps;
                yield return new WaitForEndOfFrame();
            }
            Destroy(instance);
        }
    }
}
