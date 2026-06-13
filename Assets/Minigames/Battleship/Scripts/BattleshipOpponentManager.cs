using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleshipOpponentManager : MonoBehaviour
{
    [SerializeField] private BattleshipManager manager;

    private PinHole[] holes;
    private List<int> playerHoles;
    private PinHole[] abcPlayerHoles;

    [SerializeField] private Boat[] boats;

    private Vector2Int bounds;

    private int hitRedIdx;
    private int oldHitRedIdx;
    private List<BoatRotation> notTriedDirectionsForHit;
    private BoatRotation hitRotation;
    private bool threeBoatTargetIsCorrectRotation;
    private bool hasSunkThreeBoat;
    private int boatHolesHit;

    private BoatRotation[] rotations;

    public void Setup(Vector2Int givenBounds, PinHole[] givenHoles)
    {
        bounds = givenBounds;
        holes = givenHoles;
        playerHoles = new List<int>(64);
        for (int i = 0; i < playerHoles.Capacity; i++)
            playerHoles.Add(i);
        hitRedIdx = -1;
        oldHitRedIdx = -1;
        threeBoatTargetIsCorrectRotation = false;
        hasSunkThreeBoat = false;
        hitRotation = BoatRotation.None;
        rotations = new[] { BoatRotation.Up, BoatRotation.Right, BoatRotation.Down, BoatRotation.Left };
        abcPlayerHoles = manager.abcPlayerHoles;
    }

    public void StartOpponent()
    {
        List<int> emptyHoles = new(64);
        for (int i = 0; i < emptyHoles.Capacity; i++)
            emptyHoles.Add(i);
        List<BoatRotation> baseRotations = new(4);
        baseRotations.AddRange(new[] { BoatRotation.Up, BoatRotation.Right, BoatRotation.Down, BoatRotation.Left });

        for (int i = 0; i < boats.Length; i++)
        {
            Boat boat = boats[i];
            int boatHoles = boat.GetHoles();
            int idx = -1;
            Debug.Log("Beginning with boat " + i);

            BoatRotation rotation = BoatRotation.Up;
            bool fits = false;

            while (!fits)
            {
                idx = emptyHoles[Random.Range(0, emptyHoles.Count)];
                Debug.Log("Checking on idx " + idx);
                emptyHoles.Remove(idx);
                List<BoatRotation> rotationsToTry = new(baseRotations);
                rotationsToTry = rotationsToTry.OrderBy(_ => Random.value).ToList();

                if (emptyHoles.Count == 0)
                    break;

                while (!fits && rotationsToTry.Count != 0)
                {
                    rotation = rotationsToTry[Random.Range(0, rotationsToTry.Count)];
                    rotationsToTry.Remove(rotation);
                    Debug.Log("Checking with rot " + rotation);
                    fits = CanBoatFit(idx, boatHoles, rotation);
                }
            }

            if (!fits) continue;
            Debug.Log("Fits!");
            SpawnBoat(idx, i, rotation);
            List<int> idxToRemove = new();

            switch (rotation)
            {
                case BoatRotation.Up:
                    idxToRemove.Add(idx - bounds.x);
                    if (boatHoles == 3)
                        idxToRemove.Add(idx + bounds.x);
                    break;
                case BoatRotation.Down:
                    idxToRemove.Add(idx + bounds.x);
                    if (boatHoles == 3)
                        idxToRemove.Add(idx - bounds.x);
                    break;
                case BoatRotation.Right:
                    idxToRemove.Add(idx + 1);
                    if (boatHoles == 3)
                        idxToRemove.Add(idx - 1);
                    break;
                case BoatRotation.Left:
                    idxToRemove.Add(idx - 1);
                    if (boatHoles == 3)
                        idxToRemove.Add(idx + 1);
                    break;
            }

            foreach (int t in idxToRemove)
                emptyHoles.Remove(t);
        }

        Debug.Log("Finished");
    }

    private bool CanBoatFit(int idx, int boatHoles, BoatRotation rotation)
    {
        int totalHoles = holes.Length;
        if (idx < 0 || idx > totalHoles)
            return false;

        PinHole hole = holes[idx];
        bool canSetBoat = !hole.HasBoat();
        if (!canSetBoat)
            return false;

        Vector2Int coordinates = holes[idx].GetCoords();
        Vector2Int checkCoords = rotation switch
        {
            BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y - 1),
            BoatRotation.Right => new Vector2Int(coordinates.x + 1, coordinates.y),
            BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y + 1),
            BoatRotation.Left => new Vector2Int(coordinates.x - 1, coordinates.y),
            _ => coordinates
        };

        idx = GetIdx(checkCoords);
        if (idx < 0 || idx > totalHoles)
            return false;

        if (checkCoords.x >= bounds.x || checkCoords.x < 0 || checkCoords.y >= bounds.y || checkCoords.y < 0)
            return false;

        hole = holes[idx];
        canSetBoat = !hole.HasBoat();
        if (!canSetBoat)
            return false;

        if (boatHoles != 3) return true;
        checkCoords = rotation switch
        {
            BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y + 1),
            BoatRotation.Right => new Vector2Int(coordinates.x - 1, coordinates.y),
            BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y - 1),
            BoatRotation.Left => new Vector2Int(coordinates.x + 1, coordinates.y),
            _ => coordinates
        };

        idx = GetIdx(checkCoords);
        if (idx < 0 || idx > totalHoles)
            return false;

        if (checkCoords.x >= bounds.x || checkCoords.x < 0 || checkCoords.y >= bounds.y || checkCoords.y < 0)
            return false;

        hole = holes[idx];
        canSetBoat = !hole.HasBoat();
        return canSetBoat;
    }

    private void SpawnBoat(int idx, int boatIdx, BoatRotation rotation)
    {
        Boat boat = boats[boatIdx];

        PinHole[] boatHoles = new PinHole[boat.GetHoles()];

        PinHole hole = holes[idx];
        boatHoles[0] = hole;

        int holeIdx = rotation switch
        {
            BoatRotation.Up => idx - bounds.x,
            BoatRotation.Right => idx + 1,
            BoatRotation.Down => idx + bounds.x,
            BoatRotation.Left => idx - 1,
            _ => idx
        };

        boatHoles[1] = holes[holeIdx];

        if (boatHoles.Length == 3)
        {
            holeIdx = rotation switch
            {
                BoatRotation.Up => idx + bounds.x,
                BoatRotation.Right => idx - 1,
                BoatRotation.Down => idx - bounds.x,
                BoatRotation.Left => idx + 1,
                _ => idx
            };

            boatHoles[2] = holes[holeIdx];
        }

        hole.SetBoatOpponent(boat, rotation, boatHoles);
    }

    public void StartTurn()
    {
        if (hitRedIdx == -1)
            HitRandom();
        else
            TrySinkShip();
    }

    private void HitRandom()
    {
        int idx = Random.Range(0, playerHoles.Count);
        int holeIdx = playerHoles[idx];
        Debug.Log(holeIdx + " | " + idx);
        BoatHitState hitState = manager.OpponentShoot(holeIdx);
        playerHoles.RemoveAt(idx);

        if (hitState != BoatHitState.Hit) return;
        hitRedIdx = holeIdx;
        notTriedDirectionsForHit = new List<BoatRotation>(4);
        boatHolesHit = 1;
    }

    private void TrySinkShip()
    {
        BoatRotation rotation = BoatRotation.None;
        if (notTriedDirectionsForHit.Count == 0)
            notTriedDirectionsForHit = rotations.ToList();

        int idxToHit = hitRedIdx;

        if (!hasSunkThreeBoat && hitRedIdx != -1 && oldHitRedIdx != -1)
        {
            if (threeBoatTargetIsCorrectRotation)
            {
                rotation = hitRotation;

                Vector2Int coordinates = GetCoords(idxToHit);
                Vector2Int newCoordinates = coordinates;
                switch (rotation)
                {
                    case BoatRotation.Right:
                        newCoordinates += new Vector2Int(1, 0);
                        break;
                    case BoatRotation.Left:
                        newCoordinates += new Vector2Int(-1, 0);
                        break;
                    case BoatRotation.Up:
                        newCoordinates += new Vector2Int(0, -1);
                        break;
                    case BoatRotation.Down:
                        newCoordinates += new Vector2Int(0, 1);
                        break;
                }
                switch (rotation)
                {
                    case BoatRotation.Right when newCoordinates.x >= bounds.x:
                    case BoatRotation.Left when newCoordinates.x < 0:
                    case BoatRotation.Up when newCoordinates.y < 0:
                    case BoatRotation.Down when newCoordinates.y >= bounds.y:
                        threeBoatTargetIsCorrectRotation = false;
                        break;
                }

                if (threeBoatTargetIsCorrectRotation)
                    if (!playerHoles.Contains(GetIdx(newCoordinates)))
                        threeBoatTargetIsCorrectRotation = false;
            }

            if (!threeBoatTargetIsCorrectRotation)
            {
                idxToHit = oldHitRedIdx;
                rotation = hitRotation switch
                {
                    BoatRotation.Up => BoatRotation.Down,
                    BoatRotation.Right => BoatRotation.Left,
                    BoatRotation.Down => BoatRotation.Up,
                    BoatRotation.Left => BoatRotation.Right,
                    _ => hitRotation
                };
            }

            switch (rotation)
            {
                case BoatRotation.Up:
                    idxToHit -= bounds.x;
                    break;
                case BoatRotation.Right:
                    idxToHit += 1;
                    break;
                case BoatRotation.Down:
                    idxToHit += bounds.x;
                    break;
                case BoatRotation.Left:
                    idxToHit -= 1;
                    break;
            }

            if (!playerHoles.Contains(idxToHit))
            {
                Debug.LogError("Pin already exists");
                hitRedIdx = -1;
                oldHitRedIdx = -1;
                threeBoatTargetIsCorrectRotation = false;
                hitRotation = BoatRotation.None;
                boatHolesHit = 0;
                HitRandom();
                return;
            }
        }
        else
        {
            Vector2Int coordinates = GetCoords(idxToHit);
            if (coordinates.x + 1 >= bounds.x)
                notTriedDirectionsForHit.Remove(BoatRotation.Right);
            if (coordinates.x - 1 < 0)
                notTriedDirectionsForHit.Remove(BoatRotation.Left);
            if (coordinates.y - 1 < 0)
                notTriedDirectionsForHit.Remove(BoatRotation.Up);
            if (coordinates.y + 1 >= bounds.y)
                notTriedDirectionsForHit.Remove(BoatRotation.Down);

            bool shouldLoop = true;
            while (shouldLoop)
            {
                rotation = notTriedDirectionsForHit[Random.Range(0, notTriedDirectionsForHit.Count)];
                idxToHit = hitRedIdx;

                switch (rotation)
                {
                    case BoatRotation.Up:
                        idxToHit -= bounds.x;
                        break;
                    case BoatRotation.Right:
                        idxToHit += 1;
                        break;
                    case BoatRotation.Down:
                        idxToHit += bounds.x;
                        break;
                    case BoatRotation.Left:
                        idxToHit -= 1;
                        break;
                }

                if (playerHoles.Contains(idxToHit))
                    shouldLoop = false;
                else
                    notTriedDirectionsForHit.Remove(rotation);
                if (notTriedDirectionsForHit.Count != 0) continue;
                Debug.LogError("Pin can't be placed at idx " + idxToHit);
                Debug.LogWarning("Couldn't find any directions to head with idx " + idxToHit);
                shouldLoop = false;
            }
        }

        Debug.Log("Try hitting at " + idxToHit + " with direction " + rotation);
        BoatHitState hitState = manager.OpponentShoot(idxToHit);
        notTriedDirectionsForHit.Remove(rotation);
        playerHoles.Remove(idxToHit);
        switch (hitState)
        {
            case BoatHitState.Miss:
                if (!hasSunkThreeBoat && oldHitRedIdx != -1 && hitRotation != BoatRotation.None)
                    threeBoatTargetIsCorrectRotation = false;
                break;
            case BoatHitState.Hit:
                if (!hasSunkThreeBoat && hitRedIdx != -1)
                    threeBoatTargetIsCorrectRotation = true;
                oldHitRedIdx = hitRedIdx;
                hitRedIdx = idxToHit;
                hitRotation = rotation;
                boatHolesHit++;
                break;
            case BoatHitState.Sunk:
                SunkBoat(boatHolesHit);
                break;
        }
    }

    public void SunkBoat(int boatHoles)
    {
        switch (boatHoles)
        {
            case > 3:
                return;
            case 3:
                if (hasSunkThreeBoat)
                {
                    oldHitRedIdx = -1;
                    threeBoatTargetIsCorrectRotation = false;
                    boatHolesHit = 1;
                    return;
                }
                hasSunkThreeBoat = true;
                break;
        }
        hitRedIdx = -1;
        oldHitRedIdx = -1;
        threeBoatTargetIsCorrectRotation = false;
        hitRotation = BoatRotation.None;
        boatHolesHit = 0;
    }

    private int GetIdx(Vector2Int coords)
    {
        int idx = 0;
        if (coords.y == 0)
            idx = coords.x;
        else
        {
            idx += bounds.x * coords.y;
            idx += coords.x;
        }

        return idx;
    }

    private Vector2Int GetCoords(int idx)
    {
        Vector2Int coords = new(0, 0);

        while (idx >= 8)
        {
            idx -= bounds.x;
            coords += new Vector2Int(0, 1);
        }

        if (idx > 0)
            coords += new Vector2Int(idx, 0);

        return coords;
    }
}
