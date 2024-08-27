using UnityEngine;

public class ClickPos : MonoBehaviour
{
    public Material material;
    private float t = 1.0f;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            // Mouse click position
            Vector3 mousePos = Input.mousePosition;
            Vector4 fx = new(mousePos.x / Screen.width, mousePos.y / Screen.height, 0.0f, 0.0f);
            material.SetVector("Vector2_9d795e44c7f4429ea0dea59960d1e1d1", fx);

            // Play ripple once
            t = 0.0f;
        }

        t += Time.deltaTime;
        if (t < 1.0f)
        {
            material.SetFloat("Vector1_af234c14ac3f4f1095e718c6d7a82788", t);
        }
    }
}
