using UnityEngine;

public class DustRunning : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // This gets called by your player script
    public void SetRunningDust(bool isRunning)
    {
        animator.SetBool("RunDust", isRunning);
    }
}
