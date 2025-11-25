using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuToggle : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] ThirdPersonCam thirdPersonCamera;

    bool isPaused;
    Coroutine cursorLockRoutine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (thirdPersonCamera == null)
            thirdPersonCamera = FindObjectOfType<ThirdPersonCam>();

        SetPaused(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = paused;
            canvasGroup.blocksRaycasts = paused;
            canvasGroup.alpha = paused ? 1f : 0f;
        }

        Time.timeScale = paused ? 0f : 1f;

        if (paused)
        {
            UnlockCursorForMenu();
        }
        else
        {
            LockCursorForGameplay();
        }
    }

    void UnlockCursorForMenu()
    {
        StopEnsureCursorLock();
        ApplyCursorLock(false);
    }

    void LockCursorForGameplay()
    {
        ApplyCursorLock(true);
        EnsureCursorLockNextFrame();
    }

    void ApplyCursorLock(bool locked)
    {
        if (thirdPersonCamera == null)
            thirdPersonCamera = FindObjectOfType<ThirdPersonCam>();

        if (thirdPersonCamera != null)
            thirdPersonCamera.SetCursorLocked(locked);
        else
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }

    void EnsureCursorLockNextFrame()
    {
        StopEnsureCursorLock();
        cursorLockRoutine = StartCoroutine(ReapplyCursorLock());
    }

    void StopEnsureCursorLock()
    {
        if (cursorLockRoutine != null)
        {
            StopCoroutine(cursorLockRoutine);
            cursorLockRoutine = null;
        }
    }

    IEnumerator ReapplyCursorLock()
    {
        yield return null;
        ApplyCursorLock(true);
        cursorLockRoutine = null;
    }
}
