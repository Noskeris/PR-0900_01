export const canBePlaced = (playerId, entity, cells) => {
  const withinBounds = isWithinBounds(playerId, entity, cells);
  const placeFree = isPlaceFree(entity, cells);

  const canPlace = withinBounds && placeFree;

  return canPlace;
};

export const isWithinBounds = (playerId, entity, cells) => {
  const { orientation, position, length } = entity;

  if (orientation === "vertical") {
    for (let i = 0; i < length; i++) {
      const yPos = position.y + i;
      if (
        yPos >= cells[0].length ||
        cells[position.x][yPos].ownerId !== playerId
      ) {
        return false;
      }
    }
  } else if (orientation === "horizontal") {
    for (let i = 0; i < length; i++) {
      const xPos = position.x + i;
      if (
        xPos >= cells.length ||
        cells[xPos][position.y].ownerId !== playerId
      ) {
        return false;
      }
    }
  }

  return true;
};

export const isPlaceFree = (entity, cells) => {
  const { orientation, position, length } = entity;
  if (orientation === "vertical") {
    for (let i = 0; i < length; i++) {
      const yPos = position.y + i;
      if (yPos >= cells[0].length || cells[position.x][yPos].state !== 1) {
        return false;
      }
    }
  } else if (orientation === "horizontal") {
    for (let i = 0; i < length; i++) {
      const xPos = position.x + i;
      if (xPos >= cells.length || cells[xPos][position.y].state !== 1) {
        return false;
      }
    }
  }

  return true;
};

export const calculateOverhang = (entity, cells, playerId) => {
  const { orientation, position, length } = entity;
  if (orientation === "vertical") {
    let overhang = 0;

    for (let i = 0; i < length; i++) {
      const yPos = position.y + i;
      if (
        yPos >= cells[0].length ||
        cells[position.x][yPos].ownerId !== playerId
      ) {
        overhang++;
      }
    }

    return overhang;
  } else if (orientation === "horizontal") {
    let overhang = 0;

    for (let i = 0; i < length; i++) {
      const xPos = position.x + i;
      if (
        xPos >= cells.length ||
        cells[xPos][position.y].ownerId !== playerId 
      ) {
        overhang++;
      }
    }
    return overhang;
  }

  return 0;
};

export const putEntityInLayout = (cells, entity, type) => {
  const { orientation, position, length } = entity;
  if (orientation === "vertical") {
    for (let i = 0; i < length; i++) {
      const yPos = position.y + i;
      if (yPos < cells[0].length) {
        if (cells[position.x][yPos].state !== 2) {
          cells[position.x][yPos] = {
            ...cells[position.x][yPos],
            state: type,
          };
        }
      }
    }
  } else if (orientation === "horizontal") {
    for (let i = 0; i < length; i++) {
      const xPos = position.x + i;
      if (xPos < cells.length) {
        if (cells[xPos][position.y].state !== 2) {
          cells[xPos][position.y] = {
            ...cells[xPos][position.y],
            state: type,
          };
        }
      }
    }
  }

  return cells;
};

export const calculateAttackTypeHover = (cells, playerId, x, y, attackType) => {
  const element = document.getElementById(`${x}-${y}`);
  const classList = element.classList;

  if(cells[x][y].ownerId === playerId || ((!classList.contains("defaultcell") && !classList.contains("revealedShip")) && !classList.contains("hoverOver") ))
  {
    return cells;
  }

  const updatedCells = cells.map(row => row.map(cell => ({ ...cell }))); 

  const applyHoverOver = (newX, newY) => {
    if (
      newX >= 0 &&
      newX < updatedCells.length &&         
      newY >= 0 &&
      newY < updatedCells[0].length &&     
      updatedCells[newX][newY].ownerId !== playerId 
    ) {
      const element = document.getElementById(`${newX}-${newY}`);
      const classList = element.classList;

      if (classList.contains("defaultcell") || classList.contains("hoverOver") || classList.contains("revealedShip")) {
        updatedCells[newX][newY].state = "hoverOver";
      }
    }
  };

  if (attackType === "plus") {
    const directions = [
      { dx: 0, dy: 0 },    // Center (x, y)
      { dx: -1, dy: 0 },   // Left (x-1, y)
      { dx: 1, dy: 0 },    // Right (x+1, y)
      { dx: 0, dy: -1 },   // Up (x, y-1)
      { dx: 0, dy: 1 },    // Down (x, y+1)
    ];

    directions.forEach(({ dx, dy }) => {
      const newX = x + dx;
      const newY = y + dy;
      applyHoverOver(newX, newY);
    });
  }

  if (attackType === "cross") {
    const directions = [
      { dx: 0, dy: 0 },    // Center (x, y)
      { dx: -1, dy: -1 },  // Top-left (x-1, y-1)
      { dx: 1, dy: -1 },   // Top-right (x+1, y-1)
      { dx: -1, dy: 1 },   // Bottom-left (x-1, y+1)
      { dx: 1, dy: 1 },    // Bottom-right (x+1, y+1)
    ];

    directions.forEach(({ dx, dy }) => {
      const newX = x + dx;
      const newY = y + dy;
      applyHoverOver(newX, newY);
    });
  }

  if (attackType === "boom") {
    const directions = [
      { dx: 0, dy: 0 },    // Center (x, y)
      { dx: -1, dy: -1 },  // Top-left (x-1, y-1)
      { dx: 0, dy: -1 },   // Top (x, y-1)
      { dx: 1, dy: -1 },   // Top-right (x+1, y-1)
      { dx: -1, dy: 0 },   // Left (x-1, y)
      { dx: 1, dy: 0 },    // Right (x+1, y)
      { dx: -1, dy: 1 },   // Bottom-left (x-1, y+1)
      { dx: 0, dy: 1 },    // Bottom (x, y+1)
      { dx: 1, dy: 1 },    // Bottom-right (x+1, y+1)
    ];

    directions.forEach(({ dx, dy }) => {
      const newX = x + dx;
      const newY = y + dy;
      applyHoverOver(newX, newY);
    });
  }

  return updatedCells;
};