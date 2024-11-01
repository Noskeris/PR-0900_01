import React from 'react';

const Head = ({ type }) => {
    const headImage = type === 'RoundHeaded' ? '/avatar/roundedHead.png' : '/avatar/triangleHead.png';
    return <img src={headImage} style={{ position: 'absolute', maxWidth: '200px', left: '20px', marginTop: '40px' }} />;
};

const Pimples = ({ hasPimples, isRound }) => {
    const left = isRound ? '50px' : '70px';
    const marginTop = isRound ? '160px' : '100px';
    return hasPimples ? (
        <img src="/avatar/pimples.png"  style={{ position: 'absolute', maxWidth: '30px', left: left, marginTop: marginTop }} />
    ) : null;
};

const Hair = ({ shape, isRound }) => {
    const hairImage = shape === 'Puff' ? '/avatar/puffHair.png' : '/avatar/shortHair.png';
    const left = isRound ? '70px' : '-10px';
    const marginTop = isRound ? '30px' : '35px';
    return <img src={hairImage}  style={{ position: 'absolute', maxWidth: '90px', left: left, marginTop: marginTop }} />;
};

const Cap = ({ shape, isRound }) => {
    const hairImage = shape === 'FullCap' ? '/avatar/fullCap.png' : '/avatar/hat.png';
    const left = isRound ? '50px' : '0px';
    const marginTop = isRound ? '-10px' : '0px';
    const maxWidth = isRound ? '150px' : '120px';
    const trnasform = isRound ? 'rotate(0deg)' : 'rotate(-30deg)';
    return <img src={hairImage}  style={{ position: 'absolute', maxWidth: maxWidth, left: left, marginTop: marginTop, transform: trnasform }} />;
};

const RoundSmile = ({ shape }) => {
    const hairImage = shape === 1 ? '/avatar/happySmile.png' : '/avatar/sadSmile.png';
    return <img src={hairImage}  style={{ position: 'absolute', maxWidth: '50px', left: '90px', marginTop: '160px' }} />;
};

const TriangleSmile = ({ shape }) => {
    const hairImage = shape === 3 ? '/avatar/neutralSmile.png' : '/avatar/angrySmile.png';
    return <img src={hairImage}  style={{ position: 'absolute', maxWidth: '40px', left: '140px', marginTop: '130px' }} />;
};

const AvatarComponent = ({ config }) => {
    return (
        <>
        
        <div style={{ position: 'relative', width: '200px', height: '250px', left: '60px' }}>
            {config.headType === 'RoundHeaded' &&
                <>
                
                <Head type={config.headType} />
                <Pimples hasPimples={config.hasPimples} isRound={true} />
                {config.appearance.type === 'Hair' && <Hair shape={config.appearance.shape} isRound={true} />}
                {config.appearance.type === 'Cap' && <Cap shape={config.appearance.shape} isRound={true} />}
                <RoundSmile shape={config.mood} />
                </>
            }

            {config.headType === 'TriangleHeaded' &&
                <>
                
                <Head type={config.headType} />
                <Pimples hasPimples={config.hasPimples} isRound={false} />
                {config.appearance.type === 'Hair' && <Hair shape={config.appearance.shape} isRound={false}/>}
                {config.appearance.type === 'Cap' && <Cap shape={config.appearance.shape} isRound={false}/>}
                <TriangleSmile shape={config.mood} />
                </>
            }
        
        </div>
        </>
    );
};
export default AvatarComponent;