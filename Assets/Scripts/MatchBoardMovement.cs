using System.Collections;
using UnityEngine;

public class MatchBoardMovement : MonoBehaviour
{
    private MatchBoard board;

    void Awake()
    {
        board = GetComponent<MatchBoard>();
    }

    public void MoveTile(GameObject tile, Transform targetSlot)
    {
        StartCoroutine(MoveToSlot(tile, targetSlot));
    }

    IEnumerator MoveToSlot(GameObject tile, Transform targetSlot)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();

        Vector3 startPos = rect.position;

        float time = 0f;
        float duration = 0.15f;

        while (time < duration)
        {
            if (tile == null)
                yield break;

            rect.position = Vector3.Lerp(startPos, targetSlot.position, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        rect.position = targetSlot.position;
        tile.transform.SetParent(targetSlot);
        rect.anchoredPosition = Vector2.zero;

        if (board.GetTileCount() >= board.slots.Count)
        {
            Debug.Log("Game Over");
            GameManager.instance.GameOver();
        }
    }
}
