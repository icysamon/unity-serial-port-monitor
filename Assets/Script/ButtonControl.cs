using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
    public GameObject homePanel;
    private bool homePanelState = true;

    public void HomePanelSwitch()
    {
        homePanelState = !homePanelState;
        if (homePanelState) homePanel.SetActive( true );
        else homePanel.SetActive( false );
    }
}
