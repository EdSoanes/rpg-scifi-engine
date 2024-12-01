import {
  Stat,
  StatNumber,
  StatHelpText,
  StatLabel,
  StatArrow,
  IconButton,
  useDisclosure,
  Code,
  Button,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  useColorMode,
} from '@chakra-ui/react'
import React, { useState } from 'react'
import { PropDescription } from '../../lib/rpg-api/types'
import { QuestionOutlineIcon } from '@chakra-ui/icons'
import { getPropDesc } from '../../lib/rpg-api/fetcher'
import { selectGraphState } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { PropValue } from '../../lib/rpg-api/cyborg-types'

//const describeAtom = atom<PropDesc | undefined>(undefined)

export declare interface StatPanelProps {
  propName: string
  propNameAbbr: string
  propValue?: PropValue
}

function StatPanel(props: StatPanelProps) {
  const graphState = useSelector(selectGraphState)
  const [describe, setDescribe] = useState<PropDescription | undefined>()

  const { colorMode } = useColorMode()

  const eq =
    (props?.propValue?.value ?? 0) === (props?.propValue?.baseValue ?? 0)
  const inc =
    !eq && (props?.propValue?.value ?? 0) > (props?.propValue?.baseValue ?? 0)
  const dec =
    !eq && (props?.propValue?.value ?? 0) < (props?.propValue?.baseValue ?? 0)

  const onDescribe = async () => {
    if (props?.propValue) {
      const response = await getPropDesc(
        props.propValue.id,
        'Value',
        graphState!
      )
      setDescribe(response?.data)
      onOpen()
    }
  }

  const { onOpen, onClose, isOpen } = useDisclosure()

  return (
    <>
      <Stat m={4} p={4} border="1px" borderRadius={4} borderColor={'lightgray'}>
        <StatLabel>{props.propNameAbbr}</StatLabel>
        <StatNumber>{props?.propValue?.value ?? 0}</StatNumber>
        {colorMode === 'light' && (
          <StatHelpText>
            {inc && <StatArrow type="increase" />}
            {dec && <StatArrow type="decrease" />}
            {props.propName} {props?.propValue?.baseValue ?? 0}
            <IconButton
              variant={'ghost'}
              aria-label="describe"
              size="lg"
              icon={<QuestionOutlineIcon />}
              onClick={onDescribe}
            />
          </StatHelpText>
        )}
      </Stat>
      {colorMode === 'light' && (
        <Modal isOpen={isOpen} onClose={onClose}>
          <ModalOverlay />
          <ModalContent>
            <ModalHeader>{describe?.rootProp ?? '-'}</ModalHeader>
            <ModalCloseButton />
            <ModalBody>
              <Code>{JSON.stringify(describe, null, 2)}</Code>
            </ModalBody>

            <ModalFooter>
              <Button colorScheme="blue" mr={3} onClick={onClose}>
                Close
              </Button>
            </ModalFooter>
          </ModalContent>
        </Modal>
      )}
    </>
  )
}

export default StatPanel
