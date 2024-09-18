import { Box } from "@mui/material";
import "./Board.css";

const BoardComponent = ({ board, username, playerId }) => {
  const { xLength, yLength, cells } = board;

  const playerColors = {
    1: "green",
    2: "blue",
    3: "orange",
    4: "purple",
  };

  const getCellClassName = (cell) => {
    const ownerColor = playerColors[cell.ownerId] || "defaultcell";

    switch (cell.State) {
      case "DamagedShip":
        return "damagedship";
      case "Sunken":
        return "sunken";
      case "Missed":
        return "missed";
      default:
        return `${ownerColor}`;
    }
  };

  const playerColor = playerColors[playerId] || "unknown";

  return (
    <Box sx={{ mt: 3 }}>
      <div>
        Your color is{" "}
        <strong style={{ color: playerColor }}>{playerColor}</strong>
      </div>
      <div
        className="board-grid"
        style={{
          gridTemplateColumns: `repeat(${xLength}, 40px)`,
          gridTemplateRows: `repeat(${yLength}, 40px)`,
        }}
      >
        {Array.from({ length: yLength }).map((_, yIndex) =>
          Array.from({ length: xLength }).map((_, xIndex) => (
            <div
              key={`${yIndex}-${xIndex}`}
              className={`cell ${getCellClassName(cells[xIndex][yIndex])}`}
            />
          ))
        )}
      </div>
    </Box>
  );
};

export default BoardComponent;
