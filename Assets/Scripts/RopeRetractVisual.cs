using UnityEngine;

namespace HimoHito
{
    /// <summary>A short-lived copy of the released yarn; has no joint or collider.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class RopeRetractVisual : MonoBehaviour
    {
        private const float Duration = 0.38f;
        private LineRenderer line;
        private Vector3[] offsets, drawing;
        private float[] distances;
        private Vector3 initialOrigin;
        private Material material;
        private RopeBodyVisual artwork;
        private Rigidbody2D owner;
        private float elapsed;

        public static void Play(LineRenderer source, Rigidbody2D player)
        {
            if (source == null || !source.enabled || source.positionCount < 2 || player == null) return;
            GameObject copy = new GameObject("Returning Yarn Visual");
            copy.transform.SetParent(player.transform, false);
            RopeRetractVisual effect = copy.AddComponent<RopeRetractVisual>();
            effect.owner = player;
            effect.artwork = player.GetComponent<RopeBodyVisual>();
            effect.line = copy.AddComponent<LineRenderer>();
            // The live rope clears its texture when it becomes the aim guide.
            // A private material preserves the returning yarn's texture throughout.
            if (source.sharedMaterial != null)
                effect.material = new Material(source.sharedMaterial);
            effect.line.sharedMaterial = effect.material;
            effect.line.useWorldSpace = true;
            effect.line.positionCount = source.positionCount;
            effect.line.startWidth = source.startWidth;
            effect.line.endWidth = source.endWidth;
            effect.line.startColor = source.startColor;
            effect.line.endColor = source.endColor;
            effect.line.textureMode = source.textureMode;
            effect.line.textureScale = source.textureScale;
            effect.line.numCapVertices = source.numCapVertices;
            effect.line.numCornerVertices = source.numCornerVertices;
            effect.line.sortingLayerID = source.sortingLayerID;
            effect.line.sortingOrder = source.sortingOrder;
            effect.offsets = new Vector3[source.positionCount];
            effect.drawing = new Vector3[source.positionCount];
            effect.distances = new float[source.positionCount];
            source.GetPositions(effect.drawing);
            for (int i = 0; i < effect.drawing.Length; i++)
                if (!source.useWorldSpace) effect.drawing[i] = source.transform.TransformPoint(effect.drawing[i]);
            Vector3 origin = effect.drawing[0];
            effect.initialOrigin = origin;
            for (int i = 0; i < effect.offsets.Length; i++)
            {
                effect.offsets[i] = effect.drawing[i] - origin;
                if (i > 0) effect.distances[i] = effect.distances[i - 1] +
                    Vector3.Distance(effect.drawing[i - 1], effect.drawing[i]);
            }
            effect.line.SetPositions(effect.drawing);
        }

        private void LateUpdate()
        {
            if (owner == null || !owner.simulated || !owner.gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }
            elapsed += Time.deltaTime;
            if (elapsed >= Duration) { Destroy(gameObject); return; }
            float length = 1f - Mathf.SmoothStep(0f, 1f, elapsed / Duration);
            Vector3 origin = artwork != null ? (Vector3)artwork.HeadReturnPoint : (Vector3)owner.position;
            float total = distances[distances.Length - 1];
            if (total < 0.001f) { Destroy(gameObject); return; }
            float visibleLength = total * length;
            int end = 1;
            while (end < distances.Length - 1 && distances[end] < visibleLength) end++;
            line.positionCount = end + 1;
            Vector3 movement = origin - initialOrigin;
            for (int i = 0; i < end; i++)
                line.SetPosition(i, initialOrigin + offsets[i] +
                    movement * (1f - distances[i] / total));
            float blend = Mathf.InverseLerp(distances[end - 1], distances[end], visibleLength);
            Vector3 tip = Vector3.Lerp(offsets[end - 1], offsets[end], blend);
            // Trim from the hook end along the original curve, rather than shrinking the whole curve.
            line.SetPosition(end, initialOrigin + tip + movement * (1f - visibleLength / total));
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
