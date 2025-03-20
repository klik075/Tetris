using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }//블록 정보
    public Vector3Int[] cells { get; private set; }//해당 블록 셀 정보
    public Vector3Int position { get; private set; }//블록 위치
    public int rotationIndex { get; private set; }//회전 상태

    public float stepDelay = 1f; //내려오는데 걸리는 시간
    public float lockDelay = 0.5f;//잠금하는 시간

    private float stepTime;
    private float lockTime;
    public void Initalize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        rotationIndex = 0;//회전 상태 초기화
        stepTime = Time.time + stepDelay;//다음 내려오는 시간 설정
        lockTime = 0f;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];//셀 공간만큼 마련
        }

        for(int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i]; //셀 데이터 저장
        }
    }
    private void Update()
    {
        this.board.Clear(this);//블록 모습 없앰

        lockTime += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q))//왼쪽 회전
        {
            Rotate(-1);
        }
        else if(Input.GetKeyDown(KeyCode.E))//오른쪽 회전
        {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))//왼쪽 이동
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))//오른쪽 이동
        {
            Move(Vector2Int.right);
        }

        if(Input.GetKeyDown(KeyCode.DownArrow))//한 칸 내려오기
        {
            Move(Vector2Int.down);
        }

        if(Input.GetKeyDown(KeyCode.Space))//즉시 드랍
        {
            HardDrop();
        }

        if (Time.time >= stepTime)//내려오는데 걸리는 시간 1초마다 실행
        { 
            Step();
        }

        this.board.Set(this);
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;//다음 단계 시간 설정

        Move(Vector2Int.down);//아래로 이동

        if(lockTime >= lockDelay)//잠그는 시간이 넘었으면 잠그기
        {
            Lock();
        }
    }
    private void Lock()//블록 잠금 및 새로운 블록 생성
    {
        board.Set(this);//블록 설치
        board.ClearLines();//라인 지우기
        board.SpawnPiece();//새로운 블록 생성 및 조건들 초기화
    }
    private void HardDrop()//
    {
        while(Move(Vector2Int.down))// 0, -1로 이동이 가능할 때까지 이동 후 이동 불가면 탈출
        {
            continue; 
        }

        Lock(); //잠금
    }
    private bool Move(Vector2Int translation)// 방향만큼 이동
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);//이동이 가능한지 bool값

        if(valid)//이동이 가능하면 포지션 설정
        {
            position = newPosition; //포지션을 다음 위치로 설정
            lockTime = 0f;//잠금 시간 초기화
        }

        return valid;
    }
    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);// 0 ~ 3 사이 인덱스

        ApplyRotationMatrix(direction);

        if (!TestWallkicks(rotationIndex,direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }
    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction)); 
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }
    private bool TestWallkicks(int rotationIndex, int rotationDirection)
    {
        int wallkickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallkicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallkicks[wallkickIndex, i];

            if(Move(translation))
            {
                return true;
            }
        }
        return false;
    }
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallkickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
            wallkickIndex--;

        return Wrap(wallkickIndex,0,this.data.wallkicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)//min <= input < max으로 input을 한정
    {
        if(input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
