using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateChangeListener : MonoBehaviour {

    public StateChangeListener() {
        GameStateManager.OnGameStateChange += this.respondToStateChange;
        /* The StateChangeListener is not guaranteed to be enabled when it is destroyed,
         * so it might not be able to unsubscribe itself from the OnGameStateChange event.
         * Instead, GameStateManager will unsubscribe all listeners upon a scene change. */
    }

    ///<summary>The delegate method for `GameStateManager.OnGameStateChange`.
    /// Subclasses must override this method to set their `Component`'s enabled state
    /// and/or their `GameObject`'s active state appropriately.</summary>
    public abstract void respondToStateChange(GameState newState);

}
