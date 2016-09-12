using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    public UISprite mUISprite; 
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
        {
            mUISprite.color = Color.black;
        }else if (Input.GetKey(KeyCode.A))
        {
            mUISprite.color = Color.white;
        }
    }
}
