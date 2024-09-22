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
