import {
    MainActivitiesList,
    MainAnnouncementList,
    MainIntroductionList,
    MainSidePostList,
} from "../../components/Page/guest"

const MainPage = () => {
    return (
        <>
            <MainIntroductionList />
            <div className="container-fluid">
                <div className="container">
                    <div className="row">
                        <div className="col-lg-8">
                            <MainActivitiesList />
                            <MainAnnouncementList />
                        </div>
                        <div className="col-lg-4">
                            <MainSidePostList />
                        </div>
                    </div>
                </div>
            </div>
        </>
    )
}
export default MainPage
