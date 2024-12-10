import { Button, Flex, Heading, IconButton } from '@chakra-ui/react'
import { useSelector } from 'react-redux'
import { selectTime, selectIsEncounter } from '@app/graphState/graphSelectors'
import { PointInTime } from '@lib/rpg-api/types'
import { useAppDispatch } from '@app/hooks'
import { setGraphTime } from '@app/thunks'
import {
  GiExtraTime,
  GiReturnArrow,
  GiFastForwardButton,
  GiFastBackwardButton,
} from 'react-icons/gi'

const roundIconButton = {
  borderRadius: '30px',
  backgroundColor: 'cyan.900',
  width: '60px',
  height: '60px',
  color: 'white',
}

function TimeBlock() {
  const dispatch = useAppDispatch()
  const time = useSelector(selectTime)
  const isEncounter = useSelector(selectIsEncounter)

  const onChangeEncounterTime = async () => {
    const newTime: PointInTime = isEncounter
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
  //End Enunter
  return (
    <Flex
      alignItems={'center'}
      justifyContent={'right'}
      width={'100%'}
      padding={'5px'}
    >
      <Flex
        alignItems={'center'}
        justifyContent={'left'}
        visibility={isEncounter ? 'hidden' : 'visible'}
      >
        <Button variant={'outline'} size={'sm'} onClick={onPrevTurn}>
          Minute Passes
        </Button>
        <Button variant={'outline'} size={'sm'} onClick={onNextTurn}>
          Hour Passes
        </Button>
      </Flex>

      <Flex alignItems={'center'} justifyContent={'right'} gap={'20px'}>
        {isEncounter && (
          <>
            <IconButton
              disabled={(time?.count ?? 0) < 2}
              variant={'outline'}
              size={'xs'}
              color={'gray.400'}
              onClick={onPrevTurn}
            >
              <GiFastBackwardButton />
            </IconButton>
            <Heading size={'md'}>{time?.type}</Heading>
            <Heading color={'black'} size={'xl'}>
              {time?.count}
            </Heading>
            <IconButton variant={'outline'} size={'lg'} onClick={onNextTurn}>
              <GiFastForwardButton />
            </IconButton>
          </>
        )}
        <IconButton
          css={roundIconButton}
          onClick={onChangeEncounterTime}
          size={'lg'}
          aria-label={isEncounter ? 'End encounter' : 'Start encounter'}
        >
          {isEncounter ? <GiReturnArrow /> : <GiExtraTime />}
        </IconButton>
      </Flex>
    </Flex>
  )
}

export default TimeBlock
