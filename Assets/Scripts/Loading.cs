using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField]
    private AssetReference _sceneToLoad;

    [SerializeField]
    private Image _loadingBar;

    [SerializeField]
    private TextMeshProUGUI _instruction;

    private AsyncOperationHandle<SceneInstance> _op;
    private AsyncOperation _otherOp;

    private static bool _pressedKey;


    private IEnumerator Start()
    {
        yield return null;
        _op = _sceneToLoad.LoadSceneAsync(activateOnLoad: _pressedKey);
    }

    void Update()
    {
        if (_otherOp != null)
        {
            _loadingBar.fillAmount = _otherOp.progress;
            return;
        }

        if (!_op.IsValid()) return;
        _loadingBar.fillAmount = _op.PercentComplete;

        if (!_op.IsDone) return;
        _instruction.text = "Press any key to start";
        if (!Input.anyKeyDown) return;

        _pressedKey = true;

        _otherOp = _op.Result.ActivateAsync();
    }
}
