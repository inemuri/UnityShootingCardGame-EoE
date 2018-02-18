using UnityEngine;

/*
 * 在鼠标指向地点设置一个看不见的物体方便追踪
 */
public class MouseTarget : MonoBehaviour {

    private Vector3 screenPos;

    void FixedUpdate () {
        screenPos = Input.mousePosition;
        screenPos.z = 10f - Camera.main.gameObject.transform.position.z;
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
    }
}
