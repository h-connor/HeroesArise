using UnityEditor.Animations;
using UnityEngine;
/// <summary>
/// Handles any necessary Animation utilities
/// </summary>
public static class AnimationHandler
{
    /// <summary>
    /// Returns the animation clip based on the given name, on the given layer for the animator
    /// This method will go through the animators clips for the layer, and return the correct cooresponding clip
    /// This operation runs in O(N) - Call it once and store the results if needed
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="clipName"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
   public static AnimationClip GetAnimationClipByName(Animator anim, string clipName)
    {
        if (anim != null && clipName != null)
        {
            if (anim.runtimeAnimatorController != null)
                foreach (AnimationClip i in anim.runtimeAnimatorController.animationClips)
                {
                    if (i.name.Equals(clipName))
                        return i;
                }
        }

        // No clip by that name was found
        return null;
    }
}
