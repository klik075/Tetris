using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {  get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;//블록데이터들 //인스펙터에서 드롭 할당
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);//판 사이즈

    public RectInt Bounds//판의 경계
    {
        get 
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);//사각형의 최소 x, y위치
            return new RectInt(position, boardSize);//최소 x, y 위치로부터 size만큼의 사각형 형성
        }
    }
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length;i++)
        {
            this.tetrominoes[i].Initialize();//블록데이터들 초기화 설정
        }
    }
    private void Start()
    {
        SpawnPiece();
    }
    public void SpawnPiece()//조각 생성
    {
        int random = Random.Range(0, tetrominoes.Length);//랜덤한 블록 인덱스
        TetrominoData data = this.tetrominoes[random];//해당 블록데이터

        this.activePiece.Initalize(this,this.spawnPosition, data);//현재 조각을 위치와 데이터로 설정

        if (IsValidPosition(activePiece, spawnPosition))//시작 위치에 블록을 설치할 수 있다면
        {
            Set(this.activePiece);//블록 타일 설정
        }else//아니면
        {
            GameOver();//게임 오버
        }
    }
    private void GameOver()//게임 오버
    {
        tilemap.ClearAllTiles();//타일맵의 타일들 전부 지우기
    }
    public void Set(Piece piece)//블록 타일의 현재 모습 설정
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;//조각의 위치에 셀 정보 더함
            this.tilemap.SetTile(tilePosition, piece.data.tile);//위치에 해당 타일 적용
        }
    }
    public void Clear(Piece piece)//블록 타일의 현재 모습 지움
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;//조각의 위치에 셀 정보 더함
            this.tilemap.SetTile(tilePosition, null);//타일 지움
        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)//해당 위치로 움직일 수 있으면 true
    {
        RectInt bounds = this.Bounds;//경계

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;//셀 + 위치

            if(!bounds.Contains((Vector2Int)tilePosition))//경계 안에 위치하지 않으면 true로 false 반환
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))//해당 위치에 타일이 존재하면 true로 false 반환
            {
                return false;
            }
        }

        return true;//해당 위치에 타일을 설치할 수 있다면 true
    }
    public void ClearLines()//모든 라인에 대해 지우기 실행
    {
        RectInt bounds = Bounds;//경계
        int row = bounds.yMin;//맨 밑 행에서 시작 
        
        while(row < bounds.yMax)// row < 10(최대치)
        {
            if (IsLineFull(row))//라인이 꽉 채워져 있다면
            {
                LineClear(row);//라인을 지운다.
            }
            else//지울게 없다면 행 증가
                row++;

        }
    }
    private bool IsLineFull(int row)//라인이 채워졌으면 true
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++) //-5 ~ 4 
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if(!tilemap.HasTile(position))
            {
                return false;
            }
            //Debug.Log($"{col},{row}");
        }

        return true;
    }
    private void LineClear(int row)//라인 하나 지우고 위에 라인들 내리기
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);//타일 지우기
        }

        while(row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);//해당 좌표에 있는 타일 가져오기

                position = new Vector3Int(col,row,0);//현재 행의 좌표
                tilemap.SetTile(position, above);//타일 설정하기
            }

            row++;//위에 행들도 똑같이 실시
        }
    }
}
