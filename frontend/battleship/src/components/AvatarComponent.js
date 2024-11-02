import React from 'react';

const Head = ({ type }) => {
  const headImage = type === 'RoundHeaded' ? '/avatar/roundedHead.png' : '/avatar/triangleHead.png';
  return <img src={headImage} style={{ position: 'absolute', width: '60%', left: '15%', top: '15%' }} />;
};

const Pimples = ({ hasPimples, isRound }) => {
  const left = isRound ? '20%' : '35%';
  const top = isRound ? '60%' : '40%';
  return hasPimples ? (
    <img src="/avatar/pimples.png" style={{ position: 'absolute', width: '10%', left: left, top: top }} />
  ) : null;
};

const Hair = ({ shape, isRound }) => {
  const hairImage = shape === 'Puff' ? '/avatar/puffHair.png' : '/avatar/shortHair.png';
  const left = isRound ? '30%' : '0%';
  const top = isRound ? '8%' : '13%';
  return <img src={hairImage} style={{ position: 'absolute', width: '30%', left: left, top: top }} />;
};

const Cap = ({ shape, isRound }) => {
  const capImage = shape === 'FullCap' ? '/avatar/fullCap.png' : '/avatar/hat.png';
  const left = isRound ? '27%' : '10%';
  const top = isRound ? '0' : '0%';
  return <img src={capImage} style={{ position: 'absolute', width: '35%', left: left, top: top, transform: isRound ? 'rotate(0deg)' : 'rotate(-15deg)' }} />;
};

const RoundSmile = ({ shape }) => {
  const smileImage = shape === 1 ? '/avatar/happySmile.png' : '/avatar/sadSmile.png';
  return <img src={smileImage} style={{ position: 'absolute', width: '15%', left: '37%', top: '55%' }} />;
};

const TriangleSmile = ({ shape }) => {
  const smileImage = shape === 3 ? '/avatar/neutralSmile.png' : '/avatar/angrySmile.png';
  return <img src={smileImage} style={{ position: 'absolute', width: '15%', left: '50%', top: '50%' }} />;
};

const AvatarComponent = ({ config }) => {
  return (
    <div style={{ position: 'relative', width: '100%', paddingTop: '70%', overflow: 'hidden' }}>
      {config.headType === 'RoundHeaded' && (
        <>
          <Head type={config.headType} />
          <Pimples hasPimples={config.hasPimples} isRound={true} />
          {config.appearance.type === 'Hair' && <Hair shape={config.appearance.shape} isRound={true} />}
          {config.appearance.type === 'Cap' && <Cap shape={config.appearance.shape} isRound={true} />}
          <RoundSmile shape={config.mood} />
        </>
      )}
      {config.headType === 'TriangleHeaded' && (
        <>
          <Head type={config.headType} />
          <Pimples hasPimples={config.hasPimples} isRound={false} />
          {config.appearance.type === 'Hair' && <Hair shape={config.appearance.shape} isRound={false} />}
          {config.appearance.type === 'Cap' && <Cap shape={config.appearance.shape} isRound={false} />}
          <TriangleSmile shape={config.mood} />
        </>
      )}
    </div>
  );
};

export default AvatarComponent;
