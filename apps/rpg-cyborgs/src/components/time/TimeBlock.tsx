import { Button, Heading, Stack, StackItem } from '@chakra-ui/react'
import React from 'react'
import { useSelector } from 'react-redux'
import { selectTime } from '../../app/graphState/graphSelectors'
import { PiCheckCircle, PiCross } from 'react-icons/pi'
import { PointInTime } from '../../lib/rpg-api/types'
import { useAppDispatch } from '../../app/hooks'
import { setGraphTime } from '../../app/thunks'
import { isEncounterTime } from '../../app/utils/is-encounter-time'
import { PiGreaterThan, PiLessThan } from 'react-icons/pi'

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

    dispatch(setGraphTime(newTime))
  }

  const onNextTurn = async () => {
    if (time && time?.type === 'Turn') {
      const newTime: PointInTime = {
        type: 'Turn',
        count: time.count + 1,
        isEncounterTime: true,
        isAfterEncounterTime: false,
      }
      dispatch(setGraphTime(newTime))
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
      dispatch(setGraphTime(newTime))
    }
  }
  //Start Encounter
  //Next Turn
  //Prev Turn
  //End Encounter

  return (
    <Stack direction={'row'}>
      <StackItem>
        <Heading size={'md'}>{time?.type}</Heading>
      </StackItem>
      <StackItem>
        <Button
          leftIcon={
            time?.isEncounterTime ? <PiCheckCircle /> : <PiCross />
          }
          variant={variant}
          size={'lg'}
          onClick={onChangeEncounterTime}
        >
          {isEncounterTime(time) ? 'End Encounter' : 'Begin Encounter'}
        </Button>
      </StackItem>
      <StackItem visibility={isEncounterTime(time) ? 'visible' : 'hidden'}>
        <Button
          leftIcon={<PiLessThan />}
          variant={variant}
          size={'lg'}
          onClick={onPrevTurn}
        ></Button>
        <span>{time?.count}</span>
        <Button
          leftIcon={<PiGreaterThan />}
          variant={variant}
          size={'lg'}
          onClick={onNextTurn}
        ></Button>
      </StackItem>
    </Stack>
    // <div>
    //   <
    //   <p>{time?.type}</p>
    //   {time?.isEncounterTime ? (
    //     <span>Turn: {time.count}</span>
    //   ) : (
    //     <span>{time?.type}</span>
    //   )}
    //   <Button
    //     leftIcon={
    //       time?.isEncounterTime ? <CheckCircleIcon /> : <SmallCloseIcon />
    //     }
    //     variant={variant}
    //     size={'lg'}
    //     onClick={onChangeEncounterTime}
    //   >
    //     {time?.type ?? 'BeforeTime'}
    //   </Button>

    // </div>
  )
}

export default TimeBlock
