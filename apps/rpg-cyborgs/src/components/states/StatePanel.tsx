import { useState } from 'react'
import { ModSetDescription, State } from '../../lib/rpg-api/types'
import { Grid, GridItem, Heading, Code } from '@chakra-ui/react'
import {
  selectGraphState,
  selectPlayerCharacter,
} from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '../../app/hooks'
import { toggleState } from '../../app/thunks'
import { getStateDescription } from '../../lib/rpg-api/fetcher'
import BoxButton, { BoxButtonState } from '../ui/box-button'
import { PiCheckCircleFill, PiCheckCircleLight } from 'react-icons/pi'
import {
  HoverCardArrow,
  HoverCardContent,
  HoverCardRoot,
  HoverCardTrigger,
} from '../ui/hover-card'

export declare interface StateButtonProps {
  state: State
}

function StatePanel(props: StateButtonProps) {
  const { state } = props

  const dispatch = useAppDispatch()
  const graphState = useSelector(selectGraphState)
  const playerCharacter = useSelector(selectPlayerCharacter)

  const [loadingDescribe, setLoadingDescribe] = useState<boolean>(false)
  const [describe, setDescribe] = useState<ModSetDescription | undefined>()
  const [open, setOpen] = useState(false)

  const onChangeState = (buttonState: BoxButtonState) => {
    console.log('clicked', buttonState)
    if (playerCharacter) {
      console.log('playercharacter', playerCharacter)
      dispatch(
        toggleState({
          entityId: playerCharacter.id,
          state: state.name,
          on: !state.isOn,
        })
      )
    }
  }

  const onDescribe = async (open: boolean) => {
    if (state) {
      if (!describe && !loadingDescribe) {
        setLoadingDescribe(true)
        const response = await getStateDescription(
          state.ownerId,
          state.name,
          graphState!
        )
        setDescribe(response?.data)
        setLoadingDescribe(false)
      }
      setOpen(open)
    }
  }

  return (
    <BoxButton onButtonClicked={(buttonState) => onChangeState(buttonState)}>
      <Grid templateColumns="repeat(6, 1fr)" gap={4}>
        <GridItem colSpan={5} h="10" bg="tomato">
          <HoverCardRoot
            size="sm"
            open={open}
            onOpenChange={async (e) => await onDescribe(e.open)}
          >
            <HoverCardTrigger asChild>
              <Heading as={'h3'} size={'sm'}>
                {state.name}
              </Heading>
            </HoverCardTrigger>
            <HoverCardContent>
              <HoverCardArrow />
              {describe && (
                <Code>{JSON.stringify(describe, undefined, 2)}</Code>
                // <Stack gap="4" direction="row">
                //   <PiQuestionLight />
                //   <Stack gap="3">
                //     <Stack gap="1">
                //       <Text textStyle="sm" fontWeight="semibold">
                //         Chakra UI
                //       </Text>
                //       <Text textStyle="sm" color="fg.muted">
                //         The most powerful toolkit for building modern web
                //         applications.
                //       </Text>
                //     </Stack>
                //     <HStack color="fg.subtle">
                //       <Icon size="sm">
                //         <PiCheckCircleFill />
                //       </Icon>
                //       <Text textStyle="xs">2.5M Downloads</Text>
                //     </HStack>
                //   </Stack>
                // </Stack>
              )}
            </HoverCardContent>
          </HoverCardRoot>
        </GridItem>
        <GridItem colStart={6} h="10" bg="papayawhip">
          {state.isOn ? <PiCheckCircleFill /> : <PiCheckCircleLight />}
        </GridItem>
      </Grid>
    </BoxButton>
  )

  // return (

  //     <BoxButton
  //       onButtonClicked={async (btnState) => await onChangeState(btnState)}
  //       toggle={true}
  //     >
  //       <Grid templateColumns="repeat(6, 1fr)" gap={4}>
  //         <GridItem colSpan={5} h="10" bg="tomato">
  //           <Heading as={'h3'} size={'sm'}>
  //             {props.state.name}
  //           </Heading>{' '}
  //         </GridItem>
  //         <GridItem colStart={6} h="10" bg="papayawhip">
  //           {isOn == 'on' ? <PiCheckCircleFill /> : <PiCheckCircleLight />}
  //         </GridItem>
  //       </Grid>

  //       <IconButton
  //         marginLeft={0}
  //         paddingLeft={0}
  //         aria-label="describe"
  //         size="lg"
  //         onClick={onDescribe}
  //       />
  //     </BoxButton>
  //     <Drawer.Root>
  //       <Drawer.Content>
  //         <Drawer.Header>{describe?.name ?? '-'}</Drawer.Header>
  //         <Drawer.Body>
  //           <Code>{JSON.stringify(describe, null, 2)}</Code>
  //         </Drawer.Body>

  //         <Drawer.Footer>
  //           <Button colorScheme="blue" mr={3} onClick={onClose}>
  //             Close
  //           </Button>
  //         </Drawer.Footer>
  //       </Drawer.Content>
  //     </Drawer.Root>
  //   </>
  // )
}

export default StatePanel
