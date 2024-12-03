import { Button } from '@chakra-ui/react'
import { useAppDispatch } from '../../app/hooks'
import { fetchGraphState } from '../../app/thunks'

function LoadGraphButton() {
  const dispatch = useAppDispatch()

  const onLoadGraphState = async () => {
    await dispatch(fetchGraphState('Benny'))
  }

  return (
    <div>
      <Button
        colorScheme="teal"
        variant="outline"
        onClick={async () => await onLoadGraphState()}
      >
        Get Benny
      </Button>
    </div>
  )
}

export default LoadGraphButton
