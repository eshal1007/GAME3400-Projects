using UnityEngine;

/*
 * How to use:
 * 1. create an empty game object at the destination
 * 2. on the trigger object, add a BoxCollider and check Is Trigger
 * 3. add the TeleportTrigger script to the trigger object
 * 4. drag TeleportTarget into the script's target field
 * 5. make sure the player object is tagged as Player
 */

  public class TeleportTrigger : MonoBehaviour
  {
      public Transform target; // where to teleport to
      public bool matchTargetRotation = false; // optional, rotates the player to face target rotation
      public bool disableCharacterController = true; // prevents collision issues during teleport

      private void OnTriggerEnter(Collider other)
      {
          // only react to the player
          if (!other.CompareTag("Player")) return;

          // temporarily disable character controller
          CharacterController cc = other.GetComponent<CharacterController>();
          if (disableCharacterController && cc != null)
          {
              cc.enabled = false;
          }

          // move player to the target position
          other.transform.position = target.position;
          // optional, match rotation
          if (matchTargetRotation)
          {
              other.transform.rotation = target.rotation;
          }

          // re-enable character controller after teleport
          if (disableCharacterController && cc != null)
          {
              cc.enabled = true;
          }
      }
  }