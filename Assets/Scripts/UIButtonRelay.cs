using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonRelay : MonoBehaviour
{
    
    IEnumerator EnsureGMAndInvoke(System.Action<GameManager> action)
    {
        // 1) 있으면 쓰고, 없으면 생성
        var gm = GameManager.Instance ?? FindObjectOfType<GameManager>(includeInactive: true);
        if (gm == null)
        {
            var go = new GameObject("GameManager");
            gm = go.AddComponent<GameManager>(); // Awake에서 리바인딩 수행
        }

        // 2) GM의 Awake/리바인딩이 끝나도록 한 프레임 기다림
        yield return null;

        // 3) 호출
        action?.Invoke(gm);
    }

    public void StartGame() { StartCoroutine(EnsureGMAndInvoke(gm => gm.Btn_StartGame())); }
    public void Restart()   { StartCoroutine(EnsureGMAndInvoke(gm => gm.Btn_Restart()));   }
    public void Title()     { StartCoroutine(EnsureGMAndInvoke(gm => gm.Btn_Title()));     }
}