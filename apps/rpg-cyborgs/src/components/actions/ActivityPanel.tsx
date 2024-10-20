import React from 'react'
import {
  //Code,
  Stack,
  // Tab,
  // TabList,
  // TabPanel,
  // TabPanels,
  // Tabs,
} from '@chakra-ui/react'
import ActionInstancePanel from './ActionInstancePanel'
import { selectActionInstances } from '../../app/activity/activitySelectors'
import { useSelector } from 'react-redux'
import { selectActionsStatus } from '../../app/actions/actionsSelectors'

function ActivityPanel() {

  const actionInstances = useSelector(selectActionInstances)
  const actionStatus = useSelector(selectActionsStatus)

  return (
    actionStatus === 'loaded' &&
    <Stack w={'100%'}>
      {actionInstances.map((i) => (
        <ActionInstancePanel key={(`${i.actionName}/${i.actionNo}`)} actionInstance={i} />
      ))}
    </Stack>
  )
}

export default ActivityPanel
