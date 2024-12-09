import { Box, Button, Heading, Stack } from '@chakra-ui/react'
import { useSelector } from 'react-redux'
import { selectTime } from '@app/graphState/graphSelectors'
import { PointInTime } from '@lib/rpg-api/types'
import { useAppDispatch } from '@app/hooks'
import { setGraphTime } from '@app/thunks'
import { isEncounterTime } from '@app/utils/is-encounter-time'

function TimeBlock() {
  const dispatch = useAppDispatch()
  const time = useSelector(selectTime)
  const variant = time?.isEncounterTime ? 'solid' : 'outline'

  const onChangeEncounterTime = async () => {
    const newTime: PointInTime = isEncounterTime(time)
      ? {
          type: 'Waiting',
          count: 0,
          isEncounterTime: false,
          isAfterEncounterTime: false,
        }
      : {
          type: 'Turn',
          count: 1,
          isEncounterTime: true,
          isAfterEncounterTime: false,
        }

    await dispatch(setGraphTime(newTime))
  }

  const onNextTurn = async () => {
    if (time && time?.type === 'Turn') {
      const newTime: PointInTime = {
        type: 'Turn',
        count: time.count + 1,
        isEncounterTime: true,
        isAfterEncounterTime: false,
      }
      await dispatch(setGraphTime(newTime))
    }
  }

  const onPrevTurn = async () => {
    if (time && time?.type === 'Turn' && time?.count > 1) {
      const newTime: PointInTime = {
        type: 'Turn',
        count: time.count - 1,
        isEncounterTime: true,
        isAfterEncounterTime: false,
      }
      await dispatch(setGraphTime(newTime))
    }
  }
  //Start Encounter
  //Next Turn
  //Prev Turn
  //End Encounter

  return (
    <Stack direction={'row'}>
      <div>
        <Heading size={'md'}>{time?.type}</Heading>
      </div>
      <Box>
        <Button variant={variant} size={'lg'} onClick={onChangeEncounterTime}>
          {isEncounterTime(time) ? 'End Encounter' : 'Begin Encounter'}
        </Button>
      </Box>
      <Box visibility={isEncounterTime(time) ? 'visible' : 'hidden'}>
        <Button variant={variant} size={'lg'} onClick={onPrevTurn}></Button>
        <span>{time?.count}</span>
        <Button variant={variant} size={'lg'} onClick={onNextTurn}></Button>
      </Box>
    </Stack>
  )
}

export default TimeBlock
