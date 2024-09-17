import { Table, TableBody, TableCell, TableRow, TableContainer, Paper } from '@mui/material';

const MessageContainer = ({ messages }) => {

   if (!messages || messages.length === 0) {
      return <div>No messages available</div>;
    }

  return (
    <TableContainer>
      <Table>
        <TableBody>
          {messages.map((msg, index) => (
            <TableRow key={index}>
              <TableCell>
                {msg.msg} - {msg.username}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default MessageContainer;
